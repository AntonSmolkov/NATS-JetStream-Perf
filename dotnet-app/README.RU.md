# NATS JetStreams performance testing
Создаёт `NATS JetStreams` с указанными настройками и проводит нагрузочное тестирование `(produce/consume`) на них.
`RPS` отдаются в формате `Prometheus` и на консоль.

## Environment variables

### Producer

| Параметр окружения                            | Значение по умолчанию   | Описание                                                                               |
|-----------------------------------------------|-------------------------|----------------------------------------------------------------------------------------|
| NATS_URL                                      | `nats://127.0.0.1:4222` | URL(ы) сервера(ов) NATS, к которым необходимо подключиться.                            |
| NATS_TLS_SKIP_VERIFY                          | `true`                  | Отключить проверку TLS сертификата для соединений с NATS.                              |
| NATS_TLS_CA_CERT_PATH                         | `null`                  | Путь к файлу сертификата CA для TLS-соединений.                                        |
| NATS_USERNAME                                 | `user`                  | Имя пользователя для аутентификации на сервере NATS.                                   |
| NATS_PASSWORD                                 | `password`              | Пароль для аутентификации на сервере NATS.                                             |
| NATS_PRODUCERS_ENABLED                        | `false`                 | Включить сервис `producer` сообщений.                                                  |
| NATS_PRODUCERS_PARALLEL                       | `8`                     | Количество параллельных `producer`'ов.                                                 |
| NATS_PRODUCERS_USE_DEDICATED_CONNECTIONS      | `false`                 | Использовать ли отдельное соединение с NATS для каждого из параллельных `producer`'ов. |
| NATS_PRODUCERS_BATCH_SIZE                     | `1`                     | Размер пакета сообщений отправляемых каждым из параллельных `producer`'ов.             |
| NATS_PRODUCERS_INCLUDE_MESSAGE_ID             | `false`                 | Добавлять `MsgId` к сообщениям.                                                        |
| NATS_PRODUCERS_USE_CORE_INSTEAD_OF_JETSTREAMS | `false`                 | Использовать `NATS pub/sub` вместо `JetStream` для отправки сообщений.                 |
| NATS_PRODUCERS_USE_SINGLE_SUBJECT             | `false`                 | Использовать один `subject` на `JetStream`.                                            |
| NATS_CONSUMERS_ENABLED                        | `false`                 | Включить сервис `consumer` сообщений _(не реализовано)_.                               |
| NATS_CONSUMERS_SHARED                         | `false`                 | `Consumer`ы разделяют подписки _(не реализовано)_.                                     |
| NATS_JETSTREAMS_CREATE                        | `true`                  | Создавать `JetStream` `partition`ы с указанными ниже настройками.                      |
| NATS_JETSTREAMS_USE_IN_MEMORY_STORAGE         | `false`                 | Использовать память для хранения данных `JetStreams` _(вместо диска)_.                 |
| NATS_JETSTREAMS_COMPRESSION_ENABLED           | `false`                 | Включить сжатие для `JetStreams`.                                                      |
| NATS_JETSTREAMS_REPLICAS_COUNT                | `3`                     | Количество реплик каждого `partition` `JetStream`.                                     |
| NATS_JETSTREAMS_DEDUP_WINDOW_SEC              | `300`                   | Окно дедупликации в секундах для `partition` `JetStream`.                              |
| NATS_JETSTREAMS_PARTITIONS_COUNT              | `32`                    | Количество `partition` `JetStream`.                                                    |
| NATS_JETSTREAMS_AKCS_ENABLED                  | `true`                  | Включить подтверждение сообщений на `JetStream`.                                       |


### Consumer

TBD