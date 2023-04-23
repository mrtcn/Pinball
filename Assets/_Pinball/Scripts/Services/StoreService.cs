using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Assets._Pinball.Scripts.Services
{
    public class StoreService: MonoBehaviour
    {
        public static StoreService Instance;

        private void Start()
        {
            Instance = this;
        }
        public async Task SaveAsync(Dictionary<string, object> data)
        {
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var dictionary = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { key });
            
            if (dictionary == null || !dictionary.Any()) return default(T);

            var value = JsonUtility.FromJson<T>(dictionary.Values.FirstOrDefault());
            return value;
        }
    }
}
