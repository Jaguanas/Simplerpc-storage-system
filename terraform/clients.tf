resource "kubernetes_deployment_v1" "baker" {
  metadata {
    name      = "baker"
    namespace = kubernetes_namespace_v1.experiment.metadata[0].name
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "baker"
      }
    }

    template {
      metadata {
        labels = {
          app = "baker"
        }
      }

      spec {
        container {
          name  = "baker"
          image = "jaguanas/simplerpc-baker:0.1.0"

          env {
            name  = "STORAGE_URL"
            value = "http://storage:5000/simplerpc"
          }
        }
      }
    }
  }
}

resource "kubernetes_deployment_v1" "farmer" {
  metadata {
    name      = "farmer"
    namespace = kubernetes_namespace_v1.experiment.metadata[0].name
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "farmer"
      }
    }

    template {
      metadata {
        labels = {
          app = "farmer"
        }
      }

      spec {
        container {
          name  = "farmer"
          image = "jaguanas/simplerpc-farmer:0.1.0"

          env {
            name  = "STORAGE_URL"
            value = "http://storage:5000/simplerpc"
          }
        }
      }
    }
  }
}
