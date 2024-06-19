# Reproducing NATS Jet Streams performance benchmark results discussed [here](https://github.com/nats-io/nats-server/discussions/5551)

1. Install NATS Cluster to your envinronment

    ```shell
    # Create k8s secret with dummy TLS certs (valid for 2 years)
    kubectl create secret generic nats-tls --from-file=helm/tls

    # Install NATS Helm chart with values from the discussion
    helm repo add nats https://nats-io.github.io/k8s/helm/charts
    helm upgrade --install nats nats/nats -f helm/values.yaml
    ```

2. Run NATS Bench

   ```shell
   # Open nats-box pod shell
   kubectl exec -it nats-box-{pod-tail} sh
   # Create 128 JetStreams (aka partitions)
   for i in $(seq 0 127); do
    nats stream add \
      --retention=limits \
      --storage=file \
      --replicas=3 \
      --discard=old \
      --dupe-window=2m \
      --max-age=-1 \
      --max-msgs=-1 \
      --max-bytes=-1 \
      --max-msg-size=-1 \
      --max-msgs-per-subject=-1 \
      --max-consumers=-1 \
      --allow-rollup \
      --no-deny-delete \
      --no-deny-purge \
      --subjects="events.*.$i" \
      "events-$i" > /dev/null
   done

   # Run load test profile 1 (sync). Expected result is ~35k RPS
   nats bench \
   --js \
   --multisubject \
   --pub 1280 \
   --msgs 7000000 \
   --size 100 \
   --syncpub \
   --no-progress \
   --stream "events-0" \
   "events"

   # Run load test profile 2 (async). Expected result is ~300k RPS
   nats bench \
   --js \
   --multisubject \
   --pub 1280 \
   --msgs 21000000 \
   --size 100 \
   --pubbatch 1000 \
   --no-progress \
   --stream "events-0" \
   "events"

   # Remove created JetStreams
    for i in $(seq 0 127); do nats stream rm -f "events-$i" > dev/null ;done

   ```

3. Also, sample dotnet app can be used instead of `nats-bench`. It's code, sample k8s yaml, and grafana dashboard located in the `dotnet-app` directory
