using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Geminis.Client.Modules
{
    class Resources : InterconnectClient
    {
        private Utils utils;

        private List<string> global_restart = new List<string>();
        private Dictionary<string, List<string>> resource_client_scripts = new Dictionary<string, List<string>>();
        private List<string> resources = new List<string>();

        new public void init()
        {
            utils = this.instances.Get("utils");

            for (int i = 0; i < API.GetNumResources(); i++)
            {
                string resourceName = API.GetResourceByFindIndex(i);

                if (API.GetResourceState(resourceName) == "started")
                {
                    resource_client_scripts[resourceName] = GetResourceClientScripts(resourceName);
                    resources.Add(resourceName);
                }
            }
        }

        public List<string> GetResourceClientScripts(string resourceName)
        {
            List<string> scripts = new List<string>();

            for (int i = 0; i < API.GetNumResourceMetadata(resourceName, "client_script"); i++)
            {
                scripts.Add(API.GetResourceMetadata(resourceName, "client_script", i));
            }

            return scripts;
        }

        public bool IsStarted(string resourceName)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i] == resourceName)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsNewResource(string resourceName)
        {
            for (int i = 0; i > global_restart.Count; i++)
            {
                if (global_restart[i] == resourceName)
                {
                    global_restart.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public async void OnNewResource(string resourceName)
        {
            global_restart.Add(resourceName);
        }

        public async void OnResourceStart(string resourceName)
        {
            if (!IsStarted(resourceName))
            {
                resource_client_scripts[resourceName] = GetResourceClientScripts(resourceName);
                resources.Add(resourceName);
            }

            if (API.NetworkIsPlayerActive(API.PlayerId()))
            {
                if (!IsNewResource(resourceName))
                {
                    BaseScript.TriggerServerEvent("geminis:unknown_resource", resourceName);
                }
            }
        }

        public async void OnResourceStop(string resourceName)
        {
            resources.Remove(resourceName);
            BaseScript.TriggerServerEvent("geminis:client_stop_resource", resourceName);
        }

        public async Task OnTick()
        {
            BaseScript.TriggerServerEvent("geminis:check_resources", this.resources);

            for (int i = 0; i < API.GetNumResources(); i++)
            {

                string resourceName = API.GetResourceByFindIndex(i);

                if (resourceName == null)
                {
                    break;
                }

                string resourceState = API.GetResourceState(resourceName);

                switch (resourceState)
                {
                    case "started":
                        if (!IsStarted(resourceName))
                        {
                            BaseScript.TriggerServerEvent("geminis:unknown_resource", resourceName);
                            break;
                        }

                        List<string> s_scripts = resource_client_scripts[resourceName];
                        List<string> c_scripts = GetResourceClientScripts(resourceName);

                        if (!utils.CompareLists(s_scripts, c_scripts) && API.GetResourceState(resourceName) != "starting")
                        {
                            BaseScript.TriggerServerEvent("geminis:modified_resource", resourceName);
                            break;
                        }

                        break;
                    case "stopped":
                        if (IsStarted(resourceName))
                        {
                            BaseScript.TriggerServerEvent("geminis:client_stop_resource", resourceName);
                        }

                        break;
                } 

                await BaseScript.Delay(0);
            }


            await BaseScript.Delay(4000);
        }
    }
}
