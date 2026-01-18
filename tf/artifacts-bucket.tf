variable git_branch {
  description = "GitHub repository branch"
  type = string
}

resource "aws_s3_bucket" "pipeline_artifacts_bucket" {
  bucket = "accountmanager-${var.git_branch}-pipeline-artifacts-bucket"
}
