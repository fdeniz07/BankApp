ui = true
disable_mlock = true

storage "raft" {
  # /tmp avoids Docker Desktop named-volume permission issues in lab
  path    = "/tmp/vault-raft-data"
  node_id = "vault-raft-1"
}

listener "tcp" {
  address     = "0.0.0.0:8200"
  tls_disable = 1
}

api_addr     = "http://vault-raft-1:8200"
cluster_addr = "http://vault-raft-1:8201"
