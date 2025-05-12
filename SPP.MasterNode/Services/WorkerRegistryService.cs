using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SPP.MasterNode.Models;

namespace SPP.MasterNode.Services
{
    public class WorkerRegistryService
    {
        private readonly ConcurrentDictionary<string, WorkerNode> _workers = new();
        private readonly ILogger<WorkerRegistryService> _logger;

        public WorkerRegistryService(ILogger<WorkerRegistryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public WorkerNode RegisterWorker(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Worker URL cannot be empty", nameof(url));
            }

            var worker = new WorkerNode
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                LastHeartbeat = DateTime.UtcNow,
                Status = "Active"
            };

            if (!_workers.TryAdd(worker.Id, worker))
            {
                _logger.LogWarning("Failed to register worker with URL: {Url}", url);
                throw new InvalidOperationException("Failed to register worker");
            }

            _logger.LogInformation("Worker registered successfully. ID: {Id}, URL: {Url}", worker.Id, url);
            return worker;
        }

        public bool UpdateHeartbeat(string workerId)
        {
            if (string.IsNullOrEmpty(workerId))
            {
                _logger.LogWarning("Attempted to update heartbeat with null or empty workerId");
                return false;
            }

            if (_workers.TryGetValue(workerId, out var worker))
            {
                worker.LastHeartbeat = DateTime.UtcNow;
                _logger.LogDebug("Heartbeat updated for worker {Id}", workerId);
                return true;
            }

            _logger.LogWarning("Worker not found for heartbeat update: {Id}", workerId);
            return false;
        }

        public IEnumerable<WorkerNode> GetAllWorkers()
        {
            return _workers.Values.ToList();
        }
        public WorkerNode? GetWorker(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Worker ID cannot be empty", nameof(id));
            }

            return _workers.TryGetValue(id, out var worker) ? worker : null;
        }
        public bool RemoveWorker(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Worker ID cannot be empty", nameof(id));
            }

            if (_workers.TryRemove(id, out var worker))
            {
                _logger.LogInformation("Worker removed: {Id}", id);
                return true;
            }

            _logger.LogWarning("Failed to remove worker: {Id}", id);
            return false;
        }
    }
}