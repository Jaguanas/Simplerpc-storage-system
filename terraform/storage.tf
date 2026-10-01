resource "kubernetes_deployment_v1" "storage" {
  metadata {
    name      = "storage"
    namespace = kubernetes_namespace_v1.experiment.metadata[0].name
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        app = "storage"
      }
    }

    template {
      metadata {
        labels = {
          app = "storage"
        }
      }

      spec {
        container {
          name  = "storage"
          image = "jaguanas/simplerpc-storage:0.1.0"

          port {
            container_port = 5000
          }
        }
      }
    }
  }
}

resource "kubernetes_service_v1" "storage" {
  metadata {
    name      = "storage"
    namespace = kubernetes_namespace_v1.experiment.metadata[0].name
  }

  spec {
    selector = {
      app = "storage"
    }

    port {
      port        = 5000
      target_port = 5000
    }

    type = "ClusterIP"
  }
}