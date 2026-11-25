using System;
using UnityEngine;

namespace Emilia.RemoteDebug
{
    public sealed class RemoteDebugRuntimeHost : MonoBehaviour
    {
        private static RemoteDebugRuntimeHost _instance;
        private IDisposable _native;
        public RemoteDebugServer Server { get; private set; }
        public static Action<Exception> Log { get; set; }

        public static RemoteDebugRuntimeHost EnsureStarted()
        {
            if (_instance != null) return _instance;
            // Domain Reload 后隐藏的 GameObject 可能仍在场景中，而托管 Server 已丢失。
            foreach (var stale in Resources.FindObjectsOfTypeAll<RemoteDebugRuntimeHost>())
                if (stale != null && stale.Server == null && IsOwnedHost(stale))
                    DestroyImmediate(stale.gameObject);
            var root = new GameObject("Emilia.RemoteDebug") {hideFlags = HideFlags.HideAndDontSave};
            _instance = root.AddComponent<RemoteDebugRuntimeHost>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Server = new RemoteDebugServer(() => Time.realtimeSinceStartupAsDouble, log: error => Log?.Invoke(error));
        }

        public void EnablePlayerConnection()
        {
            if (_native == null) _native = Server.AddTransport(PlayerConnectionRemoteDebugTransport.Create());
        }

        public IRemoteDebugTransport CreateLocalTransport(out IDisposable serverLease)
        {
            MemoryRemoteDebugTransport.CreatePair(out var client, out var server);
            serverLease = Server.AddTransport(server);
            return client;
        }

        private void Update() => Server?.Tick();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Shutdown();

        public static void Shutdown()
        {
            _instance = null;
            foreach (var host in Resources.FindObjectsOfTypeAll<RemoteDebugRuntimeHost>())
            {
                if (host == null || ! IsOwnedHost(host)) continue;
                host.Server?.Dispose();
                host.Server = null;
                host._native = null;
                DestroyImmediate(host.gameObject);
            }
        }

        private static bool IsOwnedHost(RemoteDebugRuntimeHost host) => host.gameObject.scene.IsValid() ||
                                                                        host.gameObject.hideFlags == HideFlags.HideAndDontSave;

        private void OnDestroy()
        {
            Server?.Dispose();
            Server = null;
            _native = null;
            if (_instance == this) _instance = null;
        }
    }
}