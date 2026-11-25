using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Emilia.RemoteDebug.Editor
{
    [InitializeOnLoad]
    public static class RemoteDebugEditorHost
    {
        private static RemoteDebugClient _native, _local;
        private static IDisposable _localLease;
        private static RemoteDebugConnectionManager _connections;
        private const string LastAddressPreference = "Emilia.RemoteDebug.LastSuccessfulAddress";
        public static Action<Exception> Log { get; set; }
        public static RemoteDebugClient ExistingClient => _native;

        public static RemoteDebugClient Client => _native ??= new RemoteDebugClient(EditorConnectionRemoteDebugTransport.Create(),
            () => EditorApplication.timeSinceStartup, log: error => Log?.Invoke(error));

        public static RemoteDebugConnectionManager Connections => _connections ??= new RemoteDebugConnectionManager(
            new UnityRemoteDebugConnectionBackend(Client), () => EditorApplication.timeSinceStartup,
            EditorPrefs.GetString(LastAddressPreference, string.Empty), address => EditorPrefs.SetString(LastAddressPreference, address));
        public static IReadOnlyList<RemoteDebugConnectionTarget> ConnectionTargets => Connections.Targets;
        public static void RefreshConnectionTargets() => Connections.Refresh();
        public static Task<RemoteDebugTarget> ConnectPlayerAsync(RemoteDebugConnectionTarget target, string serviceId, int version,
            CancellationToken cancellationToken = default) => Connections.ConnectPlayerAsync(target, serviceId, version, cancellationToken);
        public static Task<RemoteDebugTarget> ConnectAddressAsync(string address, string serviceId, int version,
            CancellationToken cancellationToken = default) => Connections.ConnectAddressAsync(address, serviceId, version, cancellationToken);

        public static RemoteDebugClient LocalClient
        {
            get
            {
                if (! EditorApplication.isPlaying) throw new InvalidOperationException("Local RemoteDebug requires Play Mode.");
                if (_local == null)
                    _local = new RemoteDebugClient(RemoteDebugRuntimeHost.EnsureStarted().CreateLocalTransport(out _localLease),
                        () => EditorApplication.timeSinceStartup, log: error => Log?.Invoke(error));
                return _local;
            }
        }

        public static RemoteDebugTarget LocalTarget => new(1, "本机 Play Mode");

        static RemoteDebugEditorHost()
        {
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
            EditorApplication.quitting += Dispose;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private static void Tick()
        {
            _native?.Tick();
            _local?.Tick();
            _connections?.Tick();
        }

        private static void PlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                DisposeLocal();
                RemoteDebugRuntimeHost.Shutdown();
            }
        }

        private static void DisposeLocal()
        {
            _local?.Dispose();
            _local = null;
            _localLease?.Dispose();
            _localLease = null;
        }

        public static void Dispose()
        {
            _connections?.Dispose();
            _connections = null;
            DisposeLocal();
            _native?.Dispose();
            _native = null;
            RemoteDebugRuntimeHost.Shutdown();
        }
    }
}
