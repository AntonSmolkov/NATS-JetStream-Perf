# NATS JetStreams Performance testing tool

Creates `NATS JetStreams` with specified settings and performs load testing (produce/consume) on them.
`RPS` are reported in Prometheus format and to the console.

## Environment Variables

### Producer

| Environment Variable                          | Default Value           | Description                                                              |
|-----------------------------------------------|-------------------------|--------------------------------------------------------------------------|
| NATS_URL                                      | `nats://127.0.0.1:4222` | The URL(s) of the NATS server(s) to connect to.                          |
| NATS_TLS_SKIP_VERIFY                          | `true`                  | Disable TLS certificate verification for NATS connections.               |
| NATS_TLS_CA_CERT_PATH                         | `null`                  | Path to the CA certificate file for TLS connections.                     |
| NATS_USERNAME                                 | `user`                  | Username for authentication on the NATS server.                          |
| NATS_PASSWORD                                 | `password`              | Password for authentication on the NATS server.                          |
| NATS_PRODUCERS_ENABLED                        | `false`                 | Enable the producer service to send messages.                            |
| NATS_PRODUCERS_PARALLEL                       | `8`                     | Number of parallel producers.                                            |
| NATS_PRODUCERS_USE_DEDICATED_CONNECTIONS      | `false`                 | Use separate connections to NATS for each parallel producer?             |
| NATS_PRODUCERS_BATCH_SIZE                     | `1`                     | Batch size of messages sent by each parallel producer.                   |
| NATS_PRODUCERS_INCLUDE_MESSAGE_ID             | `false`                 | Include MsgId in messages.                                               |
| NATS_PRODUCERS_USE_CORE_INSTEAD_OF_JETSTREAMS | `false`                 | Use `NATS pub/sub` instead of `JetStream` for sending messages.          |
| NATS_PRODUCERS_USE_SINGLE_SUBJECT             | `false`                 | Use one subject on `JetStream`.                                          |
| NATS_CONSUMERS_ENABLED                        | `false`                 | Enable the consumer service to receive messages _(not implemented yet)_. |
| NATS_CONSUMERS_SHARED                         | `false`                 | Consumers share subscriptions _(not implemented yet)_.                   |
| NATS_JETSTREAMS_CREATE                        | `true`                  | Create `JetStream` partitions with the settings below.                   |
| NATS_JETSTREAMS_USE_IN_MEMORY_STORAGE         | `false`                 | Use memory for storing `JetStreams` data _(instead of disk)_.            |
| NATS_JETSTREAMS_COMPRESSION_ENABLED           | `false`                 | Enable compression for `JetStreams`.                                     |
| NATS_JETSTREAMS_REPLICAS_COUNT                | `3`                     | Number of replicas for each `JetStream` partition.                       |
| NATS_JETSTREAMS_DEDUP_WINDOW_SEC              | `300`                   | Deduplication window in seconds for `JetStream` partitions.              |
| NATS_JETSTREAMS_PARTITIONS_COUNT              | `32`                    | Number of `JetStream` partitions.                                        |
| NATS_JETSTREAMS_AKCS_ENABLED                  | `true`                  | Enable message acknowledgment on `JetStream`.                            |

### Consumer

TBD
