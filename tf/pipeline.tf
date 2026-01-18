// Existing Terraform src code found at /tmp/terraform_src.

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

variable git_branch {
  description = "GitHub repository branch"
  type = string
}

variable git_owner {
  description = "GitHub repository owner"
  type = string
}

variable git_repo {
  description = "GitHub repository name"
  type = string
}

variable vpc_id {
  description = "The Id of the VPC to put the load balancer security group in"
  type = string
}

variable domain_name {
  description = "The base domain name that will be used for the account server.  The final name will be account.domainname.com."
  type = string
}

variable codestar_connection_arn {
  description = "codestar connection to github"
  type = string
  default = "/app/AccountManager/CodestarConnectionArn"
}

resource "aws_lambda_function" "account_manager_lambda" {
  code_signing_config_arn = {
    S3Bucket = "accountmanager-${var.git_branch}-pipeline-artifacts-bucket"
    S3Key = "AccountManager.zip"
  }
  function_name = "AccountManager"
  handler = "AccountManager"
  runtime = "dotnet6"
  timeout = 60
  memory_size = 256
  role = aws_iam_role.account_manager_lambda_execution_role.arn
}

resource "aws_lambda_permission" "account_manager_lambda_permission_for_url_invoke" {
  function_name = aws_lambda_function.account_manager_lambda.arn
  action = "lambda:InvokeFunction"
  principal = "elasticloadbalancing.amazonaws.com"
}

resource "aws_iam_role" "account_manager_lambda_execution_role" {
  assume_role_policy = {
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  }
  managed_policy_arns = [
    "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
  ]
  name = "AccountManager-${var.git_branch}-LambdaExecutionRole"
  force_detach_policies = [
    {
      PolicyName = "MyLambdaExecutionPolicy"
      PolicyDocument = {
        Version = "2012-10-17"
        Statement = [
          {
            Effect = "Allow"
            Action = [
              "logs:CreateLogGroup",
              "logs:CreateLogStream",
              "logs:PutLogEvents"
            ]
            Resource = "arn:aws:logs:*:*:log-group:/aws/lambda/AccountManager:*"
          },
          {
            Effect = "Allow"
            Action = [
              "ssm:Get*",
              "ssm:Put*"
            ]
            Resource = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter/app/AccountManager*"
          }
        ]
      }
    }
  ]
}

resource "aws_lb_target_group_attachment" "account_manager_lambda_target_group" {
  // CF Property(Name) = "AccountManagerLambdaTargetGroup"
  // CF Property(TargetType) = "lambda"
  target_id = [
    {
      Key = "lambda.multi_value_headers.enabled"
      Value = True
    }
  ]
  // CF Property(Targets) = [
  //   {
  //     Id = aws_lambda_function.account_manager_lambda.arn
  //   }
  // ]
}

resource "aws_load_balancer_listener_policy" "account_manager_load_balancer" {
  load_balancer_name = "AccountManagerLoadBalancer"
  // CF Property(Scheme) = "internet-facing"
  // CF Property(Subnets) = [
  //   "subnet-ea7bea8e",
  //   "subnet-174fe761"
  // ]
  // CF Property(Type) = "application"
  // CF Property(SecurityGroups) = [
  //   aws_security_group.account_manager_load_balancer_security_group.arn
  // ]
}

resource "aws_security_group" "account_manager_load_balancer_security_group" {
  name = "AccountManagerLoadBalancerSecurityGroup"
  description = "Allow HTTP and HTTPS traffic"
  vpc_id = var.vpc_id
  ingress = [
    {
      protocol = "tcp"
      from_port = 80
      to_port = 80
      cidr_blocks = "0.0.0.0/0"
    },
    {
      protocol = "tcp"
      from_port = 443
      to_port = 443
      cidr_blocks = "0.0.0.0/0"
    }
  ]
  egress = [
    {
      protocol = -1
      cidr_blocks = "0.0.0.0/0"
    }
  ]
}

resource "aws_transfer_certificate" "account_manager_certificate" {
  // CF Property(DomainName) = "account.${var.domain_name}"
  // CF Property(DomainValidationOptions) = [
  //   {
  //     DomainName = "account.${var.domain_name}"
  //     HostedZoneId = aws_route53_hosted_zone_dnssec.hosted_zone.id
  //   }
  // ]
  // CF Property(ValidationMethod) = "DNS"
  // CF Property(SubjectAlternativeNames) = [
  //   "*.account.${var.domain_name}"
  // ]
}

resource "aws_load_balancer_listener_policy" "https_listener" {
  load_balancer_name = aws_load_balancer_listener_policy.account_manager_load_balancer.id
  // CF Property(Protocol) = "HTTPS"
  load_balancer_port = 443
  // CF Property(DefaultActions) = [
  //   {
  //     Type = "forward"
  //     TargetGroupArn = aws_lb_target_group_attachment.account_manager_lambda_target_group.id
  //   }
  // ]
  // CF Property(Certificates) = [
  //   {
  //     CertificateArn = aws_transfer_certificate.account_manager_certificate.arn
  //   }
  // ]
}

resource "aws_load_balancer_listener_policy" "http_listener" {
  load_balancer_name = aws_load_balancer_listener_policy.account_manager_load_balancer.id
  // CF Property(Protocol) = "HTTP"
  load_balancer_port = 80
  // CF Property(DefaultActions) = [
  //   {
  //     Type = "redirect"
  //     RedirectConfig = {
  //       Protocol = "HTTPS"
  //       Port = "443"
  //       StatusCode = "HTTP_301"
  //     }
  //   }
  // ]
}

resource "aws_route53_hosted_zone_dnssec" "hosted_zone" {
  // CF Property(Name) = "account.${var.domain_name}."
}

resource "aws_route53_record" "record_set" {
  zone_id = aws_route53_hosted_zone_dnssec.hosted_zone.id
  name = "account.${var.domain_name}."
  type = "A"
  alias = {
    DNSName = aws_load_balancer_listener_policy.account_manager_load_balancer.load_balancer_name
    HostedZoneId = aws_load_balancer_listener_policy.account_manager_load_balancer.id
  }
}

resource "aws_datapipeline_pipeline" "code_pipeline" {
  // CF Property(ArtifactStore) = {
  //   Location = "accountmanager-${var.git_branch}-pipeline-artifacts-bucket"
  //   Type = "S3"
  // }
  name = "AccountManager-${var.git_branch}"
  // CF Property(RoleArn) = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/AccountManager-${var.git_branch}-CodePipelineServiceRole"
  tags = [
    {
      Name = "Source"
      Actions = [
        {
          Name = "SourceAction"
          ActionTypeId = {
            Category = "Source"
            Owner = "AWS"
            Version = 1
            Provider = "CodeStarSourceConnection"
          }
          OutputArtifacts = [
            {
              Name = "SourceOutput"
            }
          ]
          Configuration = {
            ConnectionArn = var.codestar_connection_arn
            FullRepositoryId = "${var.git_owner}/${var.git_repo}"
            BranchName = "master"
          }
          RunOrder = 1
        }
      ]
    },
    {
      Name = "UpdatePipelinePermissions"
      Actions = [
        {
          Name = "DeployAction"
          ActionTypeId = {
            Category = "Deploy"
            Owner = "AWS"
            Version = 1
            Provider = "CloudFormation"
          }
          InputArtifacts = [
            {
              Name = "SourceOutput"
            }
          ]
          Configuration = {
            ActionMode = "CREATE_UPDATE"
            StackName = "AccountManager-${var.git_branch}-iam-permissions"
            Capabilities = "CAPABILITY_IAM,CAPABILITY_NAMED_IAM"
            ParameterOverrides = "{"GitBranch": "${var.git_branch}"}"
            TemplatePath = "SourceOutput::cfn/iam-permissions.yaml"
            RoleArn = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/AccountManager-${var.git_branch}-CodePipelineServiceRole"
          }
          RunOrder = 2
        }
      ]
    },
    {
      Name = "UpdatePipeline"
      Actions = [
        {
          Name = "DeployAction"
          ActionTypeId = {
            Category = "Deploy"
            Owner = "AWS"
            Version = 1
            Provider = "CloudFormation"
          }
          InputArtifacts = [
            {
              Name = "SourceOutput"
            }
          ]
          Configuration = {
            ActionMode = "CREATE_UPDATE"
            StackName = "AccountManager-${var.git_branch}"
            Capabilities = "CAPABILITY_IAM,CAPABILITY_NAMED_IAM"
            ParameterOverrides = "{"GitOwner": "${var.git_owner}","GitRepo": "${var.git_repo}","GitBranch": "${var.git_branch}","VpcId": "${var.vpc_id}","DomainName": "${var.domain_name}"}"
            TemplatePath = "SourceOutput::cfn/pipeline.yaml"
            RoleArn = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/AccountManager-${var.git_branch}-CodePipelineServiceRole"
          }
          RunOrder = 3
        }
      ]
    },
    {
      Name = "BuildAndDeploy"
      Actions = [
        {
          Name = "BuildAction"
          ActionTypeId = {
            Category = "Build"
            Owner = "AWS"
            Version = 1
            Provider = "CodeBuild"
          }
          InputArtifacts = [
            {
              Name = "SourceOutput"
            }
          ]
          OutputArtifacts = [
            {
              Name = "BuildOutput"
            }
          ]
          Configuration = {
            ProjectName = aws_codebuild_project.code_build_project.arn
          }
          RunOrder = 4
        }
      ]
    }
  ]
}

resource "aws_codebuild_project" "code_build_project" {
  name = "AccountManager-CodeBuildProject"
  service_role = aws_iam_role.code_build_service_role.arn
  artifacts {
    type = "CODEPIPELINE"
  }
  environment {
    compute_type = "BUILD_GENERAL1_SMALL"
    image = "aws/codebuild/amazonlinux2-x86_64-standard:4.0"
    type = "LINUX_CONTAINER"
  }
  source {
    type = "CODEPIPELINE"
    buildspec = "version: 0.2
phases:
  install:
    runtime-versions:
      dotnet: 6.0
  build:
    commands:
      - dotnet publish app/AccountManager/AccountManager/AccountManager.csproj
      - (cd app/AccountManager/AccountManager/bin/Debug/net6.0/publish/ && zip -r AccountManager.zip .)
  post_build:
    commands:
      - aws s3 cp app/AccountManager/AccountManager/bin/Debug/net6.0/publish/AccountManager.zip s3://accountmanager-master-pipeline-artifacts-bucket/AccountManager.zip
      - aws lambda update-function-code --function-name AccountManager --s3-bucket accountmanager-master-pipeline-artifacts-bucket --s3-key AccountManager.zip
artifacts:
  files:
    - app/AccountManager/AccountManager/bin/Debug/net6.0/publish/AccountManager.zip
"
  }
}

resource "aws_iam_role" "code_build_service_role" {
  assume_role_policy = {
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "codebuild.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  }
  name = "AccountManager-${var.git_branch}-CodeBuildServiceRole"
  force_detach_policies = [
    {
      PolicyName = "CodeBuildServiceRolePolicy"
      PolicyDocument = {
        Version = "2012-10-17"
        Statement = [
          {
            Effect = "Allow"
            Action = [
              "logs:CreateLogGroup",
              "logs:CreateLogStream",
              "logs:PutLogEvents"
            ]
            Resource = "arn:aws:logs:*:*:*"
          },
          {
            Effect = "Allow"
            Action = [
              "s3:GetObject",
              "s3:PutObject",
              "s3:GetBucketAcl",
              "s3:GetBucketLocation",
              "s3:GetBucketPolicy",
              "s3:PutBucketPolicy",
              "s3:ListBucket",
              "s3:ListBucketMultipartUploads",
              "s3:ListMultipartUploadParts",
              "s3:AbortMultipartUpload",
              "s3:PutObjectAcl"
            ]
            Resource = "*"
          },
          {
            Effect = "Allow"
            Action = [
              "lambda:UpdateFunctionCode",
              "lambda:ListFunctions",
              "lambda:GetFunctionConfiguration"
            ]
            Resource = "arn:aws:lambda:*:*:function:AccountManager"
          }
        ]
      }
    }
  ]
}
