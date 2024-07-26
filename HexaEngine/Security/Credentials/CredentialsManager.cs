namespace HexaEngine.Security.Credentials
{
    using HexaEngine.Core.Logging;
    using HexaEngine.Core.UI;
    using System.Security;

    public static class CredentialsManager
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(CredentialsManager));
        private static CredentialsContainer? container;
        private static bool isOpen = false;
        private static SecureString? password;

        private static readonly object _lock = new();

        private const string CredentialFile = "credentials.bin";

        public static bool IsOpen => isOpen;

        public static object SyncLock => _lock;

        public static CredentialsContainer? Container => container;

        public static void AcquireLock()
        {
            Monitor.Enter(_lock);
        }

        public static void ReleaseLock()
        {
            Monitor.Exit(_lock);
        }

        public static bool TryUnlockContainer(SecureString password)
        {
            lock (_lock)
            {
                if (isOpen)
                {
                    return false;
                }

                if (!File.Exists(CredentialFile))
                {
                    try
                    {
                        container = [];
                        var fs = File.Create(CredentialFile);
                        container.Encrypt(fs, password);
                        fs.Close();
                        CredentialsManager.password = password;
                        isOpen = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to create credentials container");
                        Logger.Log(ex);
                        MessageBox.Show("Failed to create credentials container", ex.Message);
                    }
                }

                try
                {
                    container = [];
                    var fs = File.OpenRead(CredentialFile);
                    if (!container.TryDecrypt(fs, password))
                    {
                        return false;
                    }
                    fs.Close();
                    CredentialsManager.password = password;
                    isOpen = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to unlock credentials container");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to unlock credentials container", ex.Message);
                }

                return false;
            }
        }

        public static void LockContainer()
        {
            lock (_lock)
            {
                if (!isOpen || password == null || container == null)
                {
                    // just to make sure no invalid state is active
                    isOpen = false;
                    container?.Dispose();
                    container = null;
                    password?.Dispose();
                    password = null;
                    return;
                }

                try
                {
                    var fs = File.Create(CredentialFile);
                    container.Encrypt(fs, password);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save credentials container");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to save credentials container", ex.Message);
                }

                container?.Dispose();
                container = null;
                password.Dispose();
                password = null;
                isOpen = false;
            }
        }
    }
}