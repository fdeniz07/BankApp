ui = true
disable_mlock = true

storage "raft" {
  path    = "/tmp/vault-raft-data"
  node_id = "vault-raft-2"
}

listener "tcp" {
  address     = "0.0.0.0:8200"
  tls_disable = 1
}

api_addr     = "http://vault-raft-2:8200"
cluster_addr = "http://vault-raft-2:8201"
