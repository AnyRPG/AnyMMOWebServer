// Existing Terraform src code found at /tmp/terraform_src.

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

variable git_branch {
  description = "GitHub repository branch"
  type = string
}

resource "aws_iam_role" "code_pipeline_service_role" {
  assume_role_policy = {
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = [
            "codepipeline.amazonaws.com",
            "cloudformation.amazonaws.com"
          ]
        }
        Action = "sts:AssumeRole"
      }
    ]
  }
  name = "AccountManager-${var.git_branch}-CodePipelineServiceRole"
  force_detach_policies = [
    {
      PolicyName = "CodePipelineServiceRolePolicy"
      PolicyDocument = {
        Version = "2012-10-17"
        Statement = [
          {
            Effect = "Allow"
            Action = [
              "acm:DeleteCertificate",
              "acm:Describe*",
              "acm:RequestCertificate"
            ]
            Resource = "arn:aws:acm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:certificate/*"
          },
          {
            Effect = "Allow"
            Action = [
              "codebuild:BatchGetProjects"
            ]
            Resource = "*"
          },
          {
            Effect = "Allow"
            Action = [
              "codebuild:BatchGetBuilds",
              "codebuild:DeleteProject",
              "codebuild:StartBuild",
              "codebuild:UpdateProject"
            ]
            Resource = "arn:aws:codebuild:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:project/AccountManager*"
          },
          {
            Effect = "Allow"
            Action = [
              "codepipeline:DeletePipeline",
              "codepipeline:Get*",
              "codepipeline:Update*"
            ]
            Resource = "arn:aws:codepipeline:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:AccountManager-${var.git_branch}*"
          },
          {
            Effect = "Allow"
            Action = [
              "codestar-connections:UseConnection",
              "codestar-connections:PassConnection"
            ]
            Resource = "*"
          },
          {
            Effect = "Allow"
            Action = [
              "cloudformation:Describe*",
              "cloudformation:UpdateStack"
            ]
            Resource = "arn:aws:cloudformation:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:stack/AccountManager*"
          },
          {
            Effect = "Allow"
            Action = [
              "ec2:Describe*",
              "ec2:Create*",
              "ec2:AuthorizeSecurityGroupIngress",
              "ec2:AuthorizeSecurityGroupEgress",
              "ec2:DeleteSecurityGroup",
              "ec2:RevokeSecurityGroupEgress"
            ]
            Resource = "*"
          },
          {
            Effect = "Allow"
            Action = [
              "elasticloadbalancing:Describe*",
              "elasticloadbalancing:Create*",
              "elasticloadbalancing:DeleteTargetGroup",
              "elasticloadbalancing:RegisterTargets"
            ]
            Resource = "*"
          },
          {
            Effect = "Allow"
            Action = [
              "elasticloadbalancing:DeleteLoadBalancer"
            ]
            Resource = "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:loadbalancer/app/AccountManagerLoadBalancer*"
          },
          {
            Effect = "Allow"
            Action = [
              "elasticloadbalancing:DeleteListener"
            ]
            Resource = "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:listener/app/AccountManagerLoadBalancer*"
          },
          {
            Effect = "Allow"
            Action = [
              "elasticloadbalancing:ModifyTargetGroup*"
            ]
            Resource = "arn:aws:elasticloadbalancing:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:targetgroup/AccountManagerLambdaTargetGroup*"
          },
          {
            Effect = "Allow"
            Action = [
              "iam:DeleteRole",
              "iam:DeleteRolePolicy",
              "iam:DetachRolePolicy",
              "iam:Get*",
              "iam:PassRole",
              "iam:UpdateAssumeRolePolicy",
              "iam:PutRolePolicy"
            ]
            Resource = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/AccountManager*"
          },
          {
            Effect = "Allow"
            Action = [
              "lambda:Delete*",
              "lambda:Get*",
              "lambda:Add*",
              "lambda:Remove*"
            ]
            Resource = "arn:aws:lambda:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:function:AccountManager"
          },
          {
            Effect = "Allow"
            Action = [
              "route53:Create*",
              "route53:List*",
              "route53:DeleteHostedZone",
              "route53:Get*",
              "route53:ChangeResourceRecordSets"
            ]
            Resource = "*"
          },
          {
            Effect = "Allow"
            Action = [
              "s3:GetObject",
              "s3:PutObject",
              "s3:ListBucket"
            ]
            Resource = [
              "arn:aws:s3:::accountmanager-${var.git_branch}-pipeline-artifacts-bucket/*",
              "arn:aws:s3:::accountmanager-${var.git_branch}-pipeline-artifacts-bucket"
            ]
          },
          {
            Effect = "Allow"
            Action = [
              "ssm:Get*"
            ]
            Resource = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter/app/AccountManager/*"
          }
        ]
      }
    }
  ]
}

output "code_pipeline_service_role" {
  description = "The service role used for codepipeline"
  value = aws_iam_role.code_pipeline_service_role.arn
}
