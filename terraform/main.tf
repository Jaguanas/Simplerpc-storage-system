terraform {
  required_providers {
    kubernetes = {
      source = "hashicorp/kubernetes"
    }
  }
}

provider "kubernetes" {
  config_path    = pathexpand("~/.kube/config")
  config_context = "kind-simplerpc"
}

resource "kubernetes_namespace_v1" "experiment" {
  metadata {
    name = "simplerpc-tf"
  }
}