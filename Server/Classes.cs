﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Geminis.Server
{
    class InterconnectServer
    {
        Dictionary<string, dynamic> instances = new Dictionary<string, dynamic> { };
        Dictionary<string, dynamic> shared_data = new Dictionary<string, dynamic> { };
        private bool initialized = false;

        public void Add(string name, dynamic instance) {
            if (initialized) { return; }

            instances.Add(name, instance);
        }

        public void Remove(string name) {
            instances.Remove(name);
        }

        public dynamic Get(string name)
        {
            if (instances.TryGetValue(name, out _))
            {
                return instances[name];
            }

            return null;
        }

        public void Initialize()
        {
            if (initialized) { return; }

            foreach (KeyValuePair<string, dynamic> pair in this.instances) {
                pair.Value.set_instances(this);
                pair.Value.init();
            }

            foreach (KeyValuePair<string, dynamic> pair in this.instances)
            {
                Task run_task = Task.Run(() => { pair.Value.run_async(); });
                pair.Value.tasks.Add(run_task);
                pair.Value.run();
            }

            initialized = true;
        }
    }

    class InterconnectClient
    {
        public InterconnectServer instances;
        public Dictionary<string, dynamic> internal_data = new Dictionary<string, dynamic> { };
        public List<Task> tasks = new List<Task>();

        public void set_instances(InterconnectServer server)
        {
            if (this.instances != null) { return; }

            this.instances = server;
        }

        public void init() {
            return;
        }

        public async void run_async()
        {
            return;
        }

        public void run()
        {
            return;
        }

    }
}
