# Build stage
FROM golang:1.24-alpine AS builder

# Install build dependencies
RUN apk add --no-cache git protobuf protobuf-dev

# Install protoc-gen-gofast
RUN go install github.com/gogo/protobuf/protoc-gen-gofast@latest

# Set working directory
WORKDIR /workspace

# Copy the entire repository (needed for message/ proto files)
COPY . .

# Build pseudo-deepflow-agent
WORKDIR /workspace/cmd/pseudo-deepflow-agent
RUN chmod +x build.sh && ./build.sh

# Runtime stage
FROM alpine:latest

# Install ca-certificates for HTTPS (if needed in future)
RUN apk --no-cache add ca-certificates

WORKDIR /app

# Copy binary from build stage
COPY --from=builder /workspace/cmd/pseudo-deepflow-agent/pseudo-deepflow-agent /app/

# Run as non-root user
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["/app/pseudo-deepflow-agent"]
