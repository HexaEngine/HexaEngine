#pragma warning disable

using System.Runtime.InteropServices;
using System.Text;

namespace DiscordGameSdk;

internal enum Result
{
    Ok = 0,
    ServiceUnavailable = 1,
    InvalidVersion = 2,
    LockFailed = 3,
    InternalError = 4,
    InvalidPayload = 5,
    InvalidCommand = 6,
    InvalidPermissions = 7,
    NotFetched = 8,
    NotFound = 9,
    Conflict = 10,
    InvalidSecret = 11,
    InvalidJoinSecret = 12,
    NoEligibleActivity = 13,
    InvalidInvite = 14,
    NotAuthenticated = 15,
    InvalidAccessToken = 16,
    ApplicationMismatch = 17,
    InvalidDataUrl = 18,
    InvalidBase64 = 19,
    NotFiltered = 20,
    LobbyFull = 21,
    InvalidLobbySecret = 22,
    InvalidFilename = 23,
    InvalidFileSize = 24,
    InvalidEntitlement = 25,
    NotInstalled = 26,
    NotRunning = 27,
    InsufficientBuffer = 28,
    PurchaseCanceled = 29,
    InvalidGuild = 30,
    InvalidEvent = 31,
    InvalidChannel = 32,
    InvalidOrigin = 33,
    RateLimited = 34,
    OAuth2Error = 35,
    SelectChannelTimeout = 36,
    GetGuildTimeout = 37,
    SelectVoiceForceRequired = 38,
    CaptureShortcutAlreadyListening = 39,
    UnauthorizedForAchievement = 40,
    InvalidGiftCode = 41,
    PurchaseError = 42,
    TransactionAborted = 43,
    DrawingInitFailed = 44
}

internal enum CreateFlags
{
    Default = 0,
    NoRequireDiscord = 1
}

internal enum LogLevel
{
    Error = 1,
    Warn = 2,
    Info = 3,
    Debug = 4
}

internal enum UserFlag
{
    Partner = 2,
    HypeSquadEvents = 4,
    HypeSquadHouse1 = 64,
    HypeSquadHouse2 = 128,
    HypeSquadHouse3 = 256
}

internal enum PremiumType
{
    None = 0,
    Tier1 = 1,
    Tier2 = 2
}

internal enum ImageType
{
    User = 0,
}

internal enum ActivityPartyPrivacy
{
    Private = 0,
    Public = 1
}

internal enum ActivityType
{
    Playing = 0,
    Streaming = 1,
    Listening = 2,
    Watching = 3
}

internal enum ActivityActionType
{
    Join = 1,
    Spectate = 2
}

internal enum ActivitySupportedPlatformFlags
{
    Desktop = 1,
    Android = 2,
    iOS = 4
}

internal enum ActivityJoinRequestReply
{
    No = 0,
    Yes = 1,
    Ignore = 2
}

internal enum Status
{
    Offline = 0,
    Online = 1,
    Idle = 2,
    DoNotDisturb = 3
}

internal enum RelationshipType
{
    None = 0,
    Friend = 1,
    Blocked = 2,
    PendingIncoming = 3,
    PendingOutgoing = 4,
    Implicit = 5
}

internal enum LobbyType
{
    Private = 1,
    Public = 2
}

internal enum LobbySearchComparison
{
    LessThanOrEqual = -2,
    LessThan = -1,
    Equal = 0,
    GreaterThan = 1,
    GreaterThanOrEqual = 2,
    NotEqual = 3
}

internal enum LobbySearchCast
{
    String = 1,
    Number = 2
}

internal enum LobbySearchDistance
{
    Local = 0,
    Default = 1,
    Extended = 2,
    Global = 3
}

internal enum KeyVariant
{
    Normal = 0,
    Right = 1,
    Left = 2
}

internal enum MouseButton
{
    Left = 0,
    Middle = 1,
    Right = 2
}

internal enum EntitlementType
{
    Purchase = 1,
    PremiumSubscription = 2,
    DeveloperGift = 3,
    TestModePurchase = 4,
    FreePurchase = 5,
    UserGift = 6,
    PremiumPurchase = 7
}

internal enum SkuType
{
    Application = 1,
    DLC = 2,
    Consumable = 3,
    Bundle = 4
}

internal enum InputModeType
{
    VoiceActivity = 0,
    PushToTalk = 1
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct User
{
    public long Id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Username;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string Discriminator;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Avatar;

    public bool Bot;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct OAuth2Token
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string AccessToken;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public string Scopes;

    public long Expires;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ImageHandle
{
    public ImageType Type;

    public long Id;

    public uint Size;

    public static ImageHandle User(long id) => User(id, 128);

    public static ImageHandle User(long id, uint size)
    {
        return new ImageHandle
        {
            Type = ImageType.User,
            Id = id,
            Size = size,
        };
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ImageDimensions
{
    public uint Width;

    public uint Height;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ActivityTimestamps
{
    public long Start;

    public long End;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ActivityAssets
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string LargeImage;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string LargeText;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string SmallImage;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string SmallText;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct PartySize
{
    public int CurrentSize;

    public int MaxSize;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ActivityParty
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Id;

    public PartySize Size;

    public ActivityPartyPrivacy Privacy;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ActivitySecrets
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Match;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Join;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Spectate;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Activity
{
    public ActivityType Type;

    public long ApplicationId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Name;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string State;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Details;

    public ActivityTimestamps Timestamps;

    public ActivityAssets Assets;

    public ActivityParty Party;

    public ActivitySecrets Secrets;

    public bool Instance;

    public uint SupportedPlatforms;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Presence
{
    public Status Status;

    public Activity Activity;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Relationship
{
    public RelationshipType Type;

    public User User;

    public Presence Presence;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Lobby
{
    public long Id;

    public LobbyType Type;

    public long OwnerId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Secret;

    public uint Capacity;

    public bool Locked;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct ImeUnderline
{
    public int From;

    public int To;

    public uint Color;

    public uint BackgroundColor;

    public bool Thick;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Rect
{
    public int Left;

    public int Top;

    public int Right;

    public int Bottom;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct FileStat
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string Filename;

    public ulong Size;

    public ulong LastModified;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Entitlement
{
    public long Id;

    public EntitlementType Type;

    public long SkuId;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct SkuPrice
{
    public uint Amount;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string Currency;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Sku
{
    public long Id;

    public SkuType Type;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Name;

    public SkuPrice Price;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct InputMode
{
    public InputModeType Type;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Shortcut;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct UserAchievement
{
    public long UserId;

    public long AchievementId;

    public byte PercentComplete;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string UnlockedAt;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct LobbyTransaction
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetTypeMethod(nint methodsPtr, LobbyType type);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetOwnerMethod(nint methodsPtr, long ownerId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetCapacityMethod(nint methodsPtr, uint capacity);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetMetadataMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result DeleteMetadataMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetLockedMethod(nint methodsPtr, bool locked);

        public SetTypeMethod SetType;

        public SetOwnerMethod SetOwner;

        public SetCapacityMethod SetCapacity;

        public SetMetadataMethod SetMetadata;

        public DeleteMetadataMethod DeleteMetadata;

        public SetLockedMethod SetLocked;
    }

    public nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public void SetType(LobbyType type)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.SetType(MethodsPtr, type);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void SetOwner(long ownerId)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.SetOwner(MethodsPtr, ownerId);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void SetCapacity(uint capacity)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.SetCapacity(MethodsPtr, capacity);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void SetMetadata(string key, string value)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.SetMetadata(MethodsPtr, key, value);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void DeleteMetadata(string key)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.DeleteMetadata(MethodsPtr, key);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void SetLocked(bool locked)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.SetLocked(MethodsPtr, locked);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct LobbyMemberTransaction
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetMetadataMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result DeleteMetadataMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

        public SetMetadataMethod SetMetadata;

        public DeleteMetadataMethod DeleteMetadata;
    }

    public nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public void SetMetadata(string key, string value)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.SetMetadata(MethodsPtr, key, value);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void DeleteMetadata(string key)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.DeleteMetadata(MethodsPtr, key);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct LobbySearchQuery
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result FilterMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, LobbySearchComparison comparison, LobbySearchCast cast, [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SortMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, LobbySearchCast cast, [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result LimitMethod(nint methodsPtr, uint limit);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result DistanceMethod(nint methodsPtr, LobbySearchDistance distance);

        public FilterMethod Filter;

        public SortMethod Sort;

        public LimitMethod Limit;

        public DistanceMethod Distance;
    }

    public nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public void Filter(string key, LobbySearchComparison comparison, LobbySearchCast cast, string value)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.Filter(MethodsPtr, key, comparison, cast, value);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void Sort(string key, LobbySearchCast cast, string value)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.Sort(MethodsPtr, key, cast, value);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void Limit(uint limit)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.Limit(MethodsPtr, limit);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }

    public void Distance(LobbySearchDistance distance)
    {
        if (MethodsPtr != nint.Zero)
        {
            var res = Methods.Distance(MethodsPtr, distance);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }
    }
}

internal class ResultException : Exception
{
    public readonly Result Result;

    public ResultException(Result result) : base(result.ToString())
    {
    }
}

internal partial class Discord : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DestroyHandler(nint MethodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result RunCallbacksMethod(nint methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetLogHookCallback(nint ptr, LogLevel level, [MarshalAs(UnmanagedType.LPStr)] string message);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetLogHookMethod(nint methodsPtr, LogLevel minLevel, nint callbackData, SetLogHookCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetApplicationManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetUserManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetImageManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetActivityManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetRelationshipManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetLobbyManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetNetworkManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetOverlayManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetStorageManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetStoreManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetVoiceManagerMethod(nint discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint GetAchievementManagerMethod(nint discordPtr);

        public DestroyHandler Destroy;

        public RunCallbacksMethod RunCallbacks;

        public SetLogHookMethod SetLogHook;

        public GetApplicationManagerMethod GetApplicationManager;

        public GetUserManagerMethod GetUserManager;

        public GetImageManagerMethod GetImageManager;

        public GetActivityManagerMethod GetActivityManager;

        public GetRelationshipManagerMethod GetRelationshipManager;

        public GetLobbyManagerMethod GetLobbyManager;

        public GetNetworkManagerMethod GetNetworkManager;

        public GetOverlayManagerMethod GetOverlayManager;

        public GetStorageManagerMethod GetStorageManager;

        public GetStoreManagerMethod GetStoreManager;

        public GetVoiceManagerMethod GetVoiceManager;

        public GetAchievementManagerMethod GetAchievementManager;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFICreateParams
    {
        public long ClientId;

        public ulong Flags;

        public nint Events;

        public nint EventData;

        public nint ApplicationEvents;

        public uint ApplicationVersion;

        public nint UserEvents;

        public uint UserVersion;

        public nint ImageEvents;

        public uint ImageVersion;

        public nint ActivityEvents;

        public uint ActivityVersion;

        public nint RelationshipEvents;

        public uint RelationshipVersion;

        public nint LobbyEvents;

        public uint LobbyVersion;

        public nint NetworkEvents;

        public uint NetworkVersion;

        public nint OverlayEvents;

        public uint OverlayVersion;

        public nint StorageEvents;

        public uint StorageVersion;

        public nint StoreEvents;

        public uint StoreVersion;

        public nint VoiceEvents;

        public uint VoiceVersion;

        public nint AchievementEvents;

        public uint AchievementVersion;
    }

    [LibraryImport("discord_game_sdk.dll")]
    public static partial Result DiscordCreate(uint version, ref FFICreateParams createParams, out nint manager);

    public delegate void SetLogHookHandler(LogLevel level, string message);

    public GCHandle SelfHandle;

    public readonly nint EventsPtr;

    public readonly FFIEvents Events;

    public readonly nint ApplicationEventsPtr;

    public ApplicationManager.FFIEvents ApplicationEvents;

    public ApplicationManager ApplicationManagerInstance;

    public readonly nint UserEventsPtr;

    public UserManager.FFIEvents UserEvents;

    public UserManager UserManagerInstance;

    public readonly nint ImageEventsPtr;

    public ImageManager.FFIEvents ImageEvents;

    public ImageManager ImageManagerInstance;

    public readonly nint ActivityEventsPtr;

    public ActivityManager.FFIEvents ActivityEvents;

    public ActivityManager ActivityManagerInstance;

    public readonly nint RelationshipEventsPtr;

    public RelationshipManager.FFIEvents RelationshipEvents;

    public RelationshipManager RelationshipManagerInstance;

    public readonly nint LobbyEventsPtr;

    public LobbyManager.FFIEvents LobbyEvents;

    public LobbyManager LobbyManagerInstance;

    public readonly nint NetworkEventsPtr;

    public NetworkManager.FFIEvents NetworkEvents;

    public NetworkManager NetworkManagerInstance;

    public readonly nint OverlayEventsPtr;

    public OverlayManager.FFIEvents OverlayEvents;

    public OverlayManager OverlayManagerInstance;

    public readonly nint StorageEventsPtr;

    public StorageManager.FFIEvents StorageEvents;

    public StorageManager StorageManagerInstance;

    public readonly nint StoreEventsPtr;

    public StoreManager.FFIEvents StoreEvents;

    public StoreManager StoreManagerInstance;

    public readonly nint VoiceEventsPtr;

    public VoiceManager.FFIEvents VoiceEvents;

    public VoiceManager VoiceManagerInstance;

    public readonly nint AchievementEventsPtr;

    public AchievementManager.FFIEvents AchievementEvents;

    public AchievementManager AchievementManagerInstance;

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public GCHandle? setLogHook;

    public Discord(long clientId, ulong flags)
    {
        FFICreateParams createParams;
        createParams.ClientId = clientId;
        createParams.Flags = flags;
        Events = new FFIEvents();
        EventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Events));
        createParams.Events = EventsPtr;
        SelfHandle = GCHandle.Alloc(this);
        createParams.EventData = GCHandle.ToIntPtr(SelfHandle);
        ApplicationEvents = new ApplicationManager.FFIEvents();
        ApplicationEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ApplicationEvents));
        createParams.ApplicationEvents = ApplicationEventsPtr;
        createParams.ApplicationVersion = 1;
        UserEvents = new UserManager.FFIEvents();
        UserEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(UserEvents));
        createParams.UserEvents = UserEventsPtr;
        createParams.UserVersion = 1;
        ImageEvents = new ImageManager.FFIEvents();
        ImageEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ImageEvents));
        createParams.ImageEvents = ImageEventsPtr;
        createParams.ImageVersion = 1;
        ActivityEvents = new ActivityManager.FFIEvents();
        ActivityEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ActivityEvents));
        createParams.ActivityEvents = ActivityEventsPtr;
        createParams.ActivityVersion = 1;
        RelationshipEvents = new RelationshipManager.FFIEvents();
        RelationshipEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(RelationshipEvents));
        createParams.RelationshipEvents = RelationshipEventsPtr;
        createParams.RelationshipVersion = 1;
        LobbyEvents = new LobbyManager.FFIEvents();
        LobbyEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(LobbyEvents));
        createParams.LobbyEvents = LobbyEventsPtr;
        createParams.LobbyVersion = 1;
        NetworkEvents = new NetworkManager.FFIEvents();
        NetworkEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(NetworkEvents));
        createParams.NetworkEvents = NetworkEventsPtr;
        createParams.NetworkVersion = 1;
        OverlayEvents = new OverlayManager.FFIEvents();
        OverlayEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(OverlayEvents));
        createParams.OverlayEvents = OverlayEventsPtr;
        createParams.OverlayVersion = 2;
        StorageEvents = new StorageManager.FFIEvents();
        StorageEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(StorageEvents));
        createParams.StorageEvents = StorageEventsPtr;
        createParams.StorageVersion = 1;
        StoreEvents = new StoreManager.FFIEvents();
        StoreEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(StoreEvents));
        createParams.StoreEvents = StoreEventsPtr;
        createParams.StoreVersion = 1;
        VoiceEvents = new VoiceManager.FFIEvents();
        VoiceEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(VoiceEvents));
        createParams.VoiceEvents = VoiceEventsPtr;
        createParams.VoiceVersion = 1;
        AchievementEvents = new AchievementManager.FFIEvents();
        AchievementEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(AchievementEvents));
        createParams.AchievementEvents = AchievementEventsPtr;
        createParams.AchievementVersion = 1;
        InitEvents(EventsPtr, ref Events);
        var result = DiscordCreate(3, ref createParams, out MethodsPtr);
        if (result != Result.Ok)
        {
            Dispose();
            throw new ResultException(result);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events) => Marshal.StructureToPtr(events, eventsPtr, false);

    public void Dispose()
    {
        if (MethodsPtr != nint.Zero)
        {
            Methods.Destroy(MethodsPtr);
        }

        SelfHandle.Free();
        Marshal.FreeHGlobal(EventsPtr);
        Marshal.FreeHGlobal(ApplicationEventsPtr);
        Marshal.FreeHGlobal(UserEventsPtr);
        Marshal.FreeHGlobal(ImageEventsPtr);
        Marshal.FreeHGlobal(ActivityEventsPtr);
        Marshal.FreeHGlobal(RelationshipEventsPtr);
        Marshal.FreeHGlobal(LobbyEventsPtr);
        Marshal.FreeHGlobal(NetworkEventsPtr);
        Marshal.FreeHGlobal(OverlayEventsPtr);
        Marshal.FreeHGlobal(StorageEventsPtr);
        Marshal.FreeHGlobal(StoreEventsPtr);
        Marshal.FreeHGlobal(VoiceEventsPtr);
        Marshal.FreeHGlobal(AchievementEventsPtr);
        if (setLogHook.HasValue)
        {
            setLogHook.Value.Free();
        }
    }

    public void RunCallbacks()
    {
        var res = Methods.RunCallbacks(MethodsPtr);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    [MonoPInvokeCallback]
    public static void SetLogHookCallbackImpl(nint ptr, LogLevel level, string message)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SetLogHookHandler)h.Target;
        callback(level, message);
    }

    public void SetLogHook(LogLevel minLevel, SetLogHookHandler callback)
    {
        if (setLogHook.HasValue)
        {
            setLogHook.Value.Free();
        }

        setLogHook = GCHandle.Alloc(callback);
        Methods.SetLogHook(MethodsPtr, minLevel, GCHandle.ToIntPtr(setLogHook.Value), SetLogHookCallbackImpl);
    }

    public ApplicationManager GetApplicationManager()
    {
        ApplicationManagerInstance ??= new ApplicationManager(
              Methods.GetApplicationManager(MethodsPtr),
              ApplicationEventsPtr,
              ref ApplicationEvents
            );
        return ApplicationManagerInstance;
    }

    public UserManager GetUserManager()
    {
        UserManagerInstance ??= new UserManager(
              Methods.GetUserManager(MethodsPtr),
              UserEventsPtr,
              ref UserEvents
            );
        return UserManagerInstance;
    }

    public ImageManager GetImageManager()
    {
        ImageManagerInstance ??= new ImageManager(
              Methods.GetImageManager(MethodsPtr),
              ImageEventsPtr,
              ref ImageEvents
            );
        return ImageManagerInstance;
    }

    public ActivityManager GetActivityManager()
    {
        ActivityManagerInstance ??= new ActivityManager(
              Methods.GetActivityManager(MethodsPtr),
              ActivityEventsPtr,
              ref ActivityEvents
            );
        return ActivityManagerInstance;
    }

    public RelationshipManager GetRelationshipManager()
    {
        RelationshipManagerInstance ??= new RelationshipManager(
              Methods.GetRelationshipManager(MethodsPtr),
              RelationshipEventsPtr,
              ref RelationshipEvents
            );
        return RelationshipManagerInstance;
    }

    public LobbyManager GetLobbyManager()
    {
        LobbyManagerInstance ??= new LobbyManager(
              Methods.GetLobbyManager(MethodsPtr),
              LobbyEventsPtr,
              ref LobbyEvents
            );
        return LobbyManagerInstance;
    }

    public NetworkManager GetNetworkManager()
    {
        NetworkManagerInstance ??= new NetworkManager(
              Methods.GetNetworkManager(MethodsPtr),
              NetworkEventsPtr,
              ref NetworkEvents
            );
        return NetworkManagerInstance;
    }

    public OverlayManager GetOverlayManager()
    {
        OverlayManagerInstance ??= new OverlayManager(
              Methods.GetOverlayManager(MethodsPtr),
              OverlayEventsPtr,
              ref OverlayEvents
            );
        return OverlayManagerInstance;
    }

    public StorageManager GetStorageManager()
    {
        StorageManagerInstance ??= new StorageManager(
              Methods.GetStorageManager(MethodsPtr),
              StorageEventsPtr,
              ref StorageEvents
            );
        return StorageManagerInstance;
    }

    public StoreManager GetStoreManager()
    {
        StoreManagerInstance ??= new StoreManager(
              Methods.GetStoreManager(MethodsPtr),
              StoreEventsPtr,
              ref StoreEvents
            );
        return StoreManagerInstance;
    }

    public VoiceManager GetVoiceManager()
    {
        VoiceManagerInstance ??= new VoiceManager(
              Methods.GetVoiceManager(MethodsPtr),
              VoiceEventsPtr,
              ref VoiceEvents
            );
        return VoiceManagerInstance;
    }

    public AchievementManager GetAchievementManager()
    {
        AchievementManagerInstance ??= new AchievementManager(
              Methods.GetAchievementManager(MethodsPtr),
              AchievementEventsPtr,
              ref AchievementEvents
            );
        return AchievementManagerInstance;
    }
}

internal class MonoPInvokeCallbackAttribute : Attribute
{
}

internal class ApplicationManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ValidateOrExitCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ValidateOrExitMethod(nint methodsPtr, nint callbackData, ValidateOrExitCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetCurrentLocaleMethod(nint methodsPtr, StringBuilder locale);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetCurrentBranchMethod(nint methodsPtr, StringBuilder branch);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetOAuth2TokenCallback(nint ptr, Result result, ref OAuth2Token oauth2Token);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetOAuth2TokenMethod(nint methodsPtr, nint callbackData, GetOAuth2TokenCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetTicketCallback(nint ptr, Result result, [MarshalAs(UnmanagedType.LPStr)] ref string data);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetTicketMethod(nint methodsPtr, nint callbackData, GetTicketCallback callback);

        public ValidateOrExitMethod ValidateOrExit;

        public GetCurrentLocaleMethod GetCurrentLocale;

        public GetCurrentBranchMethod GetCurrentBranch;

        public GetOAuth2TokenMethod GetOAuth2Token;

        public GetTicketMethod GetTicket;
    }

    public delegate void ValidateOrExitHandler(Result result);

    public delegate void GetOAuth2TokenHandler(Result result, ref OAuth2Token oauth2Token);

    public delegate void GetTicketHandler(Result result, ref string data);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public ApplicationManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events) => Marshal.StructureToPtr(events, eventsPtr, false);

    [MonoPInvokeCallback]
    public static void ValidateOrExitCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ValidateOrExitHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void ValidateOrExit(ValidateOrExitHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ValidateOrExit(MethodsPtr, GCHandle.ToIntPtr(wrapped), ValidateOrExitCallbackImpl);
    }

    public string GetCurrentLocale()
    {
        var ret = new StringBuilder(128);
        Methods.GetCurrentLocale(MethodsPtr, ret);
        return ret.ToString();
    }

    public string GetCurrentBranch()
    {
        var ret = new StringBuilder(4096);
        Methods.GetCurrentBranch(MethodsPtr, ret);
        return ret.ToString();
    }

    [MonoPInvokeCallback]
    public static void GetOAuth2TokenCallbackImpl(nint ptr, Result result, ref OAuth2Token oauth2Token)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (GetOAuth2TokenHandler)h.Target;
        h.Free();
        callback(result, ref oauth2Token);
    }

    public void GetOAuth2Token(GetOAuth2TokenHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.GetOAuth2Token(MethodsPtr, GCHandle.ToIntPtr(wrapped), GetOAuth2TokenCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void GetTicketCallbackImpl(nint ptr, Result result, ref string data)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (GetTicketHandler)h.Target;
        h.Free();
        callback(result, ref data);
    }

    public void GetTicket(GetTicketHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.GetTicket(MethodsPtr, GCHandle.ToIntPtr(wrapped), GetTicketCallbackImpl);
    }
}

internal class UserManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CurrentUserUpdateHandler(nint ptr);

        public CurrentUserUpdateHandler OnCurrentUserUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetCurrentUserMethod(nint methodsPtr, ref User currentUser);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetUserCallback(nint ptr, Result result, ref User user);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetUserMethod(nint methodsPtr, long userId, nint callbackData, GetUserCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetCurrentUserPremiumTypeMethod(nint methodsPtr, ref PremiumType premiumType);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result CurrentUserHasFlagMethod(nint methodsPtr, UserFlag flag, ref bool hasFlag);

        public GetCurrentUserMethod GetCurrentUser;

        public GetUserMethod GetUser;

        public GetCurrentUserPremiumTypeMethod GetCurrentUserPremiumType;

        public CurrentUserHasFlagMethod CurrentUserHasFlag;
    }

    public delegate void GetUserHandler(Result result, ref User user);

    public delegate void CurrentUserUpdateHandler();

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event CurrentUserUpdateHandler OnCurrentUserUpdate;

    public UserManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnCurrentUserUpdate = OnCurrentUserUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public User GetCurrentUser()
    {
        var ret = new User();
        var res = Methods.GetCurrentUser(MethodsPtr, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void GetUserCallbackImpl(nint ptr, Result result, ref User user)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (GetUserHandler)h.Target;
        h.Free();
        callback(result, ref user);
    }

    public void GetUser(long userId, GetUserHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.GetUser(MethodsPtr, userId, GCHandle.ToIntPtr(wrapped), GetUserCallbackImpl);
    }

    public PremiumType GetCurrentUserPremiumType()
    {
        var ret = new PremiumType();
        var res = Methods.GetCurrentUserPremiumType(MethodsPtr, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public bool CurrentUserHasFlag(UserFlag flag)
    {
        bool ret = new();
        var res = Methods.CurrentUserHasFlag(MethodsPtr, flag, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void OnCurrentUserUpdateImpl(nint ptr)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.UserManagerInstance.OnCurrentUserUpdate?.Invoke();
    }
}

internal class ImageManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchCallback(nint ptr, Result result, ImageHandle handleResult);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchMethod(nint methodsPtr, ImageHandle handle, bool refresh, nint callbackData, FetchCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetDimensionsMethod(nint methodsPtr, ImageHandle handle, ref ImageDimensions dimensions);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetDataMethod(nint methodsPtr, ImageHandle handle, byte[] data, int dataLen);

        public FetchMethod Fetch;

        public GetDimensionsMethod GetDimensions;

        public GetDataMethod GetData;
    }

    public delegate void FetchHandler(Result result, ImageHandle handleResult);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public ImageManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void Fetch(ImageHandle handle, FetchHandler callback) => Fetch(handle, false, callback);

    public byte[] GetData(ImageHandle handle)
    {
        var dimensions = GetDimensions(handle);
        byte[] data = new byte[dimensions.Width * dimensions.Height * 4];
        GetData(handle, data);
        return data;
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events) => Marshal.StructureToPtr(events, eventsPtr, false);

    [MonoPInvokeCallback]
    public static void FetchCallbackImpl(nint ptr, Result result, ImageHandle handleResult)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (FetchHandler)h.Target;
        h.Free();
        callback(result, handleResult);
    }

    public void Fetch(ImageHandle handle, bool refresh, FetchHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.Fetch(MethodsPtr, handle, refresh, GCHandle.ToIntPtr(wrapped), FetchCallbackImpl);
    }

    public ImageDimensions GetDimensions(ImageHandle handle)
    {
        var ret = new ImageDimensions();
        var res = Methods.GetDimensions(MethodsPtr, handle, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public void GetData(ImageHandle handle, byte[] data)
    {
        var res = Methods.GetData(MethodsPtr, handle, data, data.Length);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }
}

internal class ActivityManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ActivityJoinHandler(nint ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ActivitySpectateHandler(nint ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ActivityJoinRequestHandler(nint ptr, ref User user);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ActivityInviteHandler(nint ptr, ActivityActionType type, ref User user, ref Activity activity);

        public ActivityJoinHandler OnActivityJoin;

        public ActivitySpectateHandler OnActivitySpectate;

        public ActivityJoinRequestHandler OnActivityJoinRequest;

        public ActivityInviteHandler OnActivityInvite;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result RegisterCommandMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string command);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result RegisterSteamMethod(nint methodsPtr, uint steamId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UpdateActivityCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UpdateActivityMethod(nint methodsPtr, ref Activity activity, nint callbackData, UpdateActivityCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ClearActivityCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ClearActivityMethod(nint methodsPtr, nint callbackData, ClearActivityCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SendRequestReplyCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SendRequestReplyMethod(nint methodsPtr, long userId, ActivityJoinRequestReply reply, nint callbackData, SendRequestReplyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SendInviteCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SendInviteMethod(nint methodsPtr, long userId, ActivityActionType type, [MarshalAs(UnmanagedType.LPStr)] string content, nint callbackData, SendInviteCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void AcceptInviteCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void AcceptInviteMethod(nint methodsPtr, long userId, nint callbackData, AcceptInviteCallback callback);

        public RegisterCommandMethod RegisterCommand;

        public RegisterSteamMethod RegisterSteam;

        public UpdateActivityMethod UpdateActivity;

        public ClearActivityMethod ClearActivity;

        public SendRequestReplyMethod SendRequestReply;

        public SendInviteMethod SendInvite;

        public AcceptInviteMethod AcceptInvite;
    }

    public delegate void UpdateActivityHandler(Result result);

    public delegate void ClearActivityHandler(Result result);

    public delegate void SendRequestReplyHandler(Result result);

    public delegate void SendInviteHandler(Result result);

    public delegate void AcceptInviteHandler(Result result);

    public delegate void ActivityJoinHandler(string secret);

    public delegate void ActivitySpectateHandler(string secret);

    public delegate void ActivityJoinRequestHandler(ref User user);

    public delegate void ActivityInviteHandler(ActivityActionType type, ref User user, ref Activity activity);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event ActivityJoinHandler OnActivityJoin;

    public event ActivitySpectateHandler OnActivitySpectate;

    public event ActivityJoinRequestHandler OnActivityJoinRequest;

    public event ActivityInviteHandler OnActivityInvite;

    public ActivityManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnActivityJoin = OnActivityJoinImpl;
        events.OnActivitySpectate = OnActivitySpectateImpl;
        events.OnActivityJoinRequest = OnActivityJoinRequestImpl;
        events.OnActivityInvite = OnActivityInviteImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public void RegisterCommand(string command)
    {
        var res = Methods.RegisterCommand(MethodsPtr, command);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public void RegisterCommand() => RegisterCommand(null);

    public void RegisterSteam(uint steamId)
    {
        var res = Methods.RegisterSteam(MethodsPtr, steamId);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    [MonoPInvokeCallback]
    public static void UpdateActivityCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (UpdateActivityHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void UpdateActivity(Activity activity, UpdateActivityHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.UpdateActivity(MethodsPtr, ref activity, GCHandle.ToIntPtr(wrapped), UpdateActivityCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void ClearActivityCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ClearActivityHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void ClearActivity(ClearActivityHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ClearActivity(MethodsPtr, GCHandle.ToIntPtr(wrapped), ClearActivityCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void SendRequestReplyCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SendRequestReplyHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SendRequestReply(long userId, ActivityJoinRequestReply reply, SendRequestReplyHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SendRequestReply(MethodsPtr, userId, reply, GCHandle.ToIntPtr(wrapped), SendRequestReplyCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void SendInviteCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SendInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SendInvite(long userId, ActivityActionType type, string content, SendInviteHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SendInvite(MethodsPtr, userId, type, content, GCHandle.ToIntPtr(wrapped), SendInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void AcceptInviteCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (AcceptInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void AcceptInvite(long userId, AcceptInviteHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.AcceptInvite(MethodsPtr, userId, GCHandle.ToIntPtr(wrapped), AcceptInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void OnActivityJoinImpl(nint ptr, string secret)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.ActivityManagerInstance.OnActivityJoin?.Invoke(secret);
    }

    [MonoPInvokeCallback]
    public static void OnActivitySpectateImpl(nint ptr, string secret)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.ActivityManagerInstance.OnActivitySpectate?.Invoke(secret);
    }

    [MonoPInvokeCallback]
    public static void OnActivityJoinRequestImpl(nint ptr, ref User user)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.ActivityManagerInstance.OnActivityJoinRequest?.Invoke(ref user);
    }

    [MonoPInvokeCallback]
    public static void OnActivityInviteImpl(nint ptr, ActivityActionType type, ref User user, ref Activity activity)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.ActivityManagerInstance.OnActivityInvite?.Invoke(type, ref user, ref activity);
    }
}

internal class RelationshipManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void RefreshHandler(nint ptr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void RelationshipUpdateHandler(nint ptr, ref Relationship relationship);

        public RefreshHandler OnRefresh;

        public RelationshipUpdateHandler OnRelationshipUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool FilterCallback(nint ptr, ref Relationship relationship);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FilterMethod(nint methodsPtr, nint callbackData, FilterCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result CountMethod(nint methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetMethod(nint methodsPtr, long userId, ref Relationship relationship);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetAtMethod(nint methodsPtr, uint index, ref Relationship relationship);

        public FilterMethod Filter;

        public CountMethod Count;

        public GetMethod Get;

        public GetAtMethod GetAt;
    }

    public delegate bool FilterHandler(ref Relationship relationship);

    public delegate void RefreshHandler();

    public delegate void RelationshipUpdateHandler(ref Relationship relationship);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event RefreshHandler OnRefresh;

    public event RelationshipUpdateHandler OnRelationshipUpdate;

    public RelationshipManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnRefresh = OnRefreshImpl;
        events.OnRelationshipUpdate = OnRelationshipUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    public static bool FilterCallbackImpl(nint ptr, ref Relationship relationship)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (FilterHandler)h.Target;
        return callback(ref relationship);
    }

    public void Filter(FilterHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.Filter(MethodsPtr, GCHandle.ToIntPtr(wrapped), FilterCallbackImpl);
        wrapped.Free();
    }

    public int Count()
    {
        int ret = new();
        var res = Methods.Count(MethodsPtr, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public Relationship Get(long userId)
    {
        var ret = new Relationship();
        var res = Methods.Get(MethodsPtr, userId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public Relationship GetAt(uint index)
    {
        var ret = new Relationship();
        var res = Methods.GetAt(MethodsPtr, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void OnRefreshImpl(nint ptr)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.RelationshipManagerInstance.OnRefresh?.Invoke();
    }

    [MonoPInvokeCallback]
    public static void OnRelationshipUpdateImpl(nint ptr, ref Relationship relationship)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.RelationshipManagerInstance.OnRelationshipUpdate?.Invoke(ref relationship);
    }
}

internal class LobbyManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void LobbyUpdateHandler(nint ptr, long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void LobbyDeleteHandler(nint ptr, long lobbyId, uint reason);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void MemberConnectHandler(nint ptr, long lobbyId, long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void MemberUpdateHandler(nint ptr, long lobbyId, long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void MemberDisconnectHandler(nint ptr, long lobbyId, long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void LobbyMessageHandler(nint ptr, long lobbyId, long userId, nint dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SpeakingHandler(nint ptr, long lobbyId, long userId, bool speaking);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void NetworkMessageHandler(nint ptr, long lobbyId, long userId, byte channelId, nint dataPtr, int dataLen);

        public LobbyUpdateHandler OnLobbyUpdate;

        public LobbyDeleteHandler OnLobbyDelete;

        public MemberConnectHandler OnMemberConnect;

        public MemberUpdateHandler OnMemberUpdate;

        public MemberDisconnectHandler OnMemberDisconnect;

        public LobbyMessageHandler OnLobbyMessage;

        public SpeakingHandler OnSpeaking;

        public NetworkMessageHandler OnNetworkMessage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyCreateTransactionMethod(nint methodsPtr, ref nint transaction);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyUpdateTransactionMethod(nint methodsPtr, long lobbyId, ref nint transaction);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetMemberUpdateTransactionMethod(nint methodsPtr, long lobbyId, long userId, ref nint transaction);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CreateLobbyCallback(nint ptr, Result result, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CreateLobbyMethod(nint methodsPtr, nint transaction, nint callbackData, CreateLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UpdateLobbyCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UpdateLobbyMethod(nint methodsPtr, long lobbyId, nint transaction, nint callbackData, UpdateLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DeleteLobbyCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DeleteLobbyMethod(nint methodsPtr, long lobbyId, nint callbackData, DeleteLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ConnectLobbyCallback(nint ptr, Result result, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ConnectLobbyMethod(nint methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string secret, nint callbackData, ConnectLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ConnectLobbyWithActivitySecretCallback(nint ptr, Result result, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ConnectLobbyWithActivitySecretMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string activitySecret, nint callbackData, ConnectLobbyWithActivitySecretCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DisconnectLobbyCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DisconnectLobbyMethod(nint methodsPtr, long lobbyId, nint callbackData, DisconnectLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyMethod(nint methodsPtr, long lobbyId, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyActivitySecretMethod(nint methodsPtr, long lobbyId, StringBuilder secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyMetadataValueMethod(nint methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyMetadataKeyMethod(nint methodsPtr, long lobbyId, int index, StringBuilder key);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result LobbyMetadataCountMethod(nint methodsPtr, long lobbyId, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result MemberCountMethod(nint methodsPtr, long lobbyId, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetMemberUserIdMethod(nint methodsPtr, long lobbyId, int index, ref long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetMemberUserMethod(nint methodsPtr, long lobbyId, long userId, ref User user);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetMemberMetadataValueMethod(nint methodsPtr, long lobbyId, long userId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetMemberMetadataKeyMethod(nint methodsPtr, long lobbyId, long userId, int index, StringBuilder key);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result MemberMetadataCountMethod(nint methodsPtr, long lobbyId, long userId, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UpdateMemberCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UpdateMemberMethod(nint methodsPtr, long lobbyId, long userId, nint transaction, nint callbackData, UpdateMemberCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SendLobbyMessageCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SendLobbyMessageMethod(nint methodsPtr, long lobbyId, byte[] data, int dataLen, nint callbackData, SendLobbyMessageCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetSearchQueryMethod(nint methodsPtr, ref nint query);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SearchCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SearchMethod(nint methodsPtr, nint query, nint callbackData, SearchCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void LobbyCountMethod(nint methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLobbyIdMethod(nint methodsPtr, int index, ref long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ConnectVoiceCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ConnectVoiceMethod(nint methodsPtr, long lobbyId, nint callbackData, ConnectVoiceCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DisconnectVoiceCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DisconnectVoiceMethod(nint methodsPtr, long lobbyId, nint callbackData, DisconnectVoiceCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result ConnectNetworkMethod(nint methodsPtr, long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result DisconnectNetworkMethod(nint methodsPtr, long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result FlushNetworkMethod(nint methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result OpenNetworkChannelMethod(nint methodsPtr, long lobbyId, byte channelId, bool reliable);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SendNetworkMessageMethod(nint methodsPtr, long lobbyId, long userId, byte channelId, byte[] data, int dataLen);

        public GetLobbyCreateTransactionMethod GetLobbyCreateTransaction;

        public GetLobbyUpdateTransactionMethod GetLobbyUpdateTransaction;

        public GetMemberUpdateTransactionMethod GetMemberUpdateTransaction;

        public CreateLobbyMethod CreateLobby;

        public UpdateLobbyMethod UpdateLobby;

        public DeleteLobbyMethod DeleteLobby;

        public ConnectLobbyMethod ConnectLobby;

        public ConnectLobbyWithActivitySecretMethod ConnectLobbyWithActivitySecret;

        public DisconnectLobbyMethod DisconnectLobby;

        public GetLobbyMethod GetLobby;

        public GetLobbyActivitySecretMethod GetLobbyActivitySecret;

        public GetLobbyMetadataValueMethod GetLobbyMetadataValue;

        public GetLobbyMetadataKeyMethod GetLobbyMetadataKey;

        public LobbyMetadataCountMethod LobbyMetadataCount;

        public MemberCountMethod MemberCount;

        public GetMemberUserIdMethod GetMemberUserId;

        public GetMemberUserMethod GetMemberUser;

        public GetMemberMetadataValueMethod GetMemberMetadataValue;

        public GetMemberMetadataKeyMethod GetMemberMetadataKey;

        public MemberMetadataCountMethod MemberMetadataCount;

        public UpdateMemberMethod UpdateMember;

        public SendLobbyMessageMethod SendLobbyMessage;

        public GetSearchQueryMethod GetSearchQuery;

        public SearchMethod Search;

        public LobbyCountMethod LobbyCount;

        public GetLobbyIdMethod GetLobbyId;

        public ConnectVoiceMethod ConnectVoice;

        public DisconnectVoiceMethod DisconnectVoice;

        public ConnectNetworkMethod ConnectNetwork;

        public DisconnectNetworkMethod DisconnectNetwork;

        public FlushNetworkMethod FlushNetwork;

        public OpenNetworkChannelMethod OpenNetworkChannel;

        public SendNetworkMessageMethod SendNetworkMessage;
    }

    public delegate void CreateLobbyHandler(Result result, ref Lobby lobby);

    public delegate void UpdateLobbyHandler(Result result);

    public delegate void DeleteLobbyHandler(Result result);

    public delegate void ConnectLobbyHandler(Result result, ref Lobby lobby);

    public delegate void ConnectLobbyWithActivitySecretHandler(Result result, ref Lobby lobby);

    public delegate void DisconnectLobbyHandler(Result result);

    public delegate void UpdateMemberHandler(Result result);

    public delegate void SendLobbyMessageHandler(Result result);

    public delegate void SearchHandler(Result result);

    public delegate void ConnectVoiceHandler(Result result);

    public delegate void DisconnectVoiceHandler(Result result);

    public delegate void LobbyUpdateHandler(long lobbyId);

    public delegate void LobbyDeleteHandler(long lobbyId, uint reason);

    public delegate void MemberConnectHandler(long lobbyId, long userId);

    public delegate void MemberUpdateHandler(long lobbyId, long userId);

    public delegate void MemberDisconnectHandler(long lobbyId, long userId);

    public delegate void LobbyMessageHandler(long lobbyId, long userId, byte[] data);

    public delegate void SpeakingHandler(long lobbyId, long userId, bool speaking);

    public delegate void NetworkMessageHandler(long lobbyId, long userId, byte channelId, byte[] data);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event LobbyUpdateHandler OnLobbyUpdate;

    public event LobbyDeleteHandler OnLobbyDelete;

    public event MemberConnectHandler OnMemberConnect;

    public event MemberUpdateHandler OnMemberUpdate;

    public event MemberDisconnectHandler OnMemberDisconnect;

    public event LobbyMessageHandler OnLobbyMessage;

    public event SpeakingHandler OnSpeaking;

    public event NetworkMessageHandler OnNetworkMessage;

    public LobbyManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public IEnumerable<User> GetMemberUsers(long lobbyID)
    {
        int memberCount = MemberCount(lobbyID);
        var members = new List<User>();
        for (int i = 0; i < memberCount; i++)
        {
            members.Add(GetMemberUser(lobbyID, GetMemberUserId(lobbyID, i)));
        }

        return members;
    }

    public void SendLobbyMessage(long lobbyID, string data, SendLobbyMessageHandler handler) => SendLobbyMessage(lobbyID, Encoding.UTF8.GetBytes(data), handler);

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnLobbyUpdate = OnLobbyUpdateImpl;
        events.OnLobbyDelete = OnLobbyDeleteImpl;
        events.OnMemberConnect = OnMemberConnectImpl;
        events.OnMemberUpdate = OnMemberUpdateImpl;
        events.OnMemberDisconnect = OnMemberDisconnectImpl;
        events.OnLobbyMessage = OnLobbyMessageImpl;
        events.OnSpeaking = OnSpeakingImpl;
        events.OnNetworkMessage = OnNetworkMessageImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public LobbyTransaction GetLobbyCreateTransaction()
    {
        var ret = new LobbyTransaction();
        var res = Methods.GetLobbyCreateTransaction(MethodsPtr, ref ret.MethodsPtr);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public LobbyTransaction GetLobbyUpdateTransaction(long lobbyId)
    {
        var ret = new LobbyTransaction();
        var res = Methods.GetLobbyUpdateTransaction(MethodsPtr, lobbyId, ref ret.MethodsPtr);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public LobbyMemberTransaction GetMemberUpdateTransaction(long lobbyId, long userId)
    {
        var ret = new LobbyMemberTransaction();
        var res = Methods.GetMemberUpdateTransaction(MethodsPtr, lobbyId, userId, ref ret.MethodsPtr);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void CreateLobbyCallbackImpl(nint ptr, Result result, ref Lobby lobby)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (CreateLobbyHandler)h.Target;
        h.Free();
        callback(result, ref lobby);
    }

    public void CreateLobby(LobbyTransaction transaction, CreateLobbyHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.CreateLobby(MethodsPtr, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), CreateLobbyCallbackImpl);
        transaction.MethodsPtr = nint.Zero;
    }

    [MonoPInvokeCallback]
    public static void UpdateLobbyCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (UpdateLobbyHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void UpdateLobby(long lobbyId, LobbyTransaction transaction, UpdateLobbyHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.UpdateLobby(MethodsPtr, lobbyId, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), UpdateLobbyCallbackImpl);
        transaction.MethodsPtr = nint.Zero;
    }

    [MonoPInvokeCallback]
    public static void DeleteLobbyCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (DeleteLobbyHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void DeleteLobby(long lobbyId, DeleteLobbyHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.DeleteLobby(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DeleteLobbyCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void ConnectLobbyCallbackImpl(nint ptr, Result result, ref Lobby lobby)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ConnectLobbyHandler)h.Target;
        h.Free();
        callback(result, ref lobby);
    }

    public void ConnectLobby(long lobbyId, string secret, ConnectLobbyHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ConnectLobby(MethodsPtr, lobbyId, secret, GCHandle.ToIntPtr(wrapped), ConnectLobbyCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void ConnectLobbyWithActivitySecretCallbackImpl(nint ptr, Result result, ref Lobby lobby)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ConnectLobbyWithActivitySecretHandler)h.Target;
        h.Free();
        callback(result, ref lobby);
    }

    public void ConnectLobbyWithActivitySecret(string activitySecret, ConnectLobbyWithActivitySecretHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ConnectLobbyWithActivitySecret(MethodsPtr, activitySecret, GCHandle.ToIntPtr(wrapped), ConnectLobbyWithActivitySecretCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void DisconnectLobbyCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (DisconnectLobbyHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void DisconnectLobby(long lobbyId, DisconnectLobbyHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.DisconnectLobby(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DisconnectLobbyCallbackImpl);
    }

    public Lobby GetLobby(long lobbyId)
    {
        var ret = new Lobby();
        var res = Methods.GetLobby(MethodsPtr, lobbyId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public string GetLobbyActivitySecret(long lobbyId)
    {
        var ret = new StringBuilder(128);
        var res = Methods.GetLobbyActivitySecret(MethodsPtr, lobbyId, ret);
        return res != Result.Ok ? throw new ResultException(res) : ret.ToString();
    }

    public string GetLobbyMetadataValue(long lobbyId, string key)
    {
        var ret = new StringBuilder(4096);
        var res = Methods.GetLobbyMetadataValue(MethodsPtr, lobbyId, key, ret);
        return res != Result.Ok ? throw new ResultException(res) : ret.ToString();
    }

    public string GetLobbyMetadataKey(long lobbyId, int index)
    {
        var ret = new StringBuilder(256);
        var res = Methods.GetLobbyMetadataKey(MethodsPtr, lobbyId, index, ret);
        return res != Result.Ok ? throw new ResultException(res) : ret.ToString();
    }

    public int LobbyMetadataCount(long lobbyId)
    {
        int ret = new();
        var res = Methods.LobbyMetadataCount(MethodsPtr, lobbyId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public int MemberCount(long lobbyId)
    {
        int ret = new();
        var res = Methods.MemberCount(MethodsPtr, lobbyId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public long GetMemberUserId(long lobbyId, int index)
    {
        long ret = new();
        var res = Methods.GetMemberUserId(MethodsPtr, lobbyId, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public User GetMemberUser(long lobbyId, long userId)
    {
        var ret = new User();
        var res = Methods.GetMemberUser(MethodsPtr, lobbyId, userId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public string GetMemberMetadataValue(long lobbyId, long userId, string key)
    {
        var ret = new StringBuilder(4096);
        var res = Methods.GetMemberMetadataValue(MethodsPtr, lobbyId, userId, key, ret);
        return res != Result.Ok ? throw new ResultException(res) : ret.ToString();
    }

    public string GetMemberMetadataKey(long lobbyId, long userId, int index)
    {
        var ret = new StringBuilder(256);
        var res = Methods.GetMemberMetadataKey(MethodsPtr, lobbyId, userId, index, ret);
        return res != Result.Ok ? throw new ResultException(res) : ret.ToString();
    }

    public int MemberMetadataCount(long lobbyId, long userId)
    {
        int ret = new();
        var res = Methods.MemberMetadataCount(MethodsPtr, lobbyId, userId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void UpdateMemberCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (UpdateMemberHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void UpdateMember(long lobbyId, long userId, LobbyMemberTransaction transaction, UpdateMemberHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.UpdateMember(MethodsPtr, lobbyId, userId, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), UpdateMemberCallbackImpl);
        transaction.MethodsPtr = nint.Zero;
    }

    [MonoPInvokeCallback]
    public static void SendLobbyMessageCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SendLobbyMessageHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SendLobbyMessage(long lobbyId, byte[] data, SendLobbyMessageHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SendLobbyMessage(MethodsPtr, lobbyId, data, data.Length, GCHandle.ToIntPtr(wrapped), SendLobbyMessageCallbackImpl);
    }

    public LobbySearchQuery GetSearchQuery()
    {
        var ret = new LobbySearchQuery();
        var res = Methods.GetSearchQuery(MethodsPtr, ref ret.MethodsPtr);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void SearchCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SearchHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void Search(LobbySearchQuery query, SearchHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.Search(MethodsPtr, query.MethodsPtr, GCHandle.ToIntPtr(wrapped), SearchCallbackImpl);
        query.MethodsPtr = nint.Zero;
    }

    public int LobbyCount()
    {
        int ret = new();
        Methods.LobbyCount(MethodsPtr, ref ret);
        return ret;
    }

    public long GetLobbyId(int index)
    {
        long ret = new();
        var res = Methods.GetLobbyId(MethodsPtr, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void ConnectVoiceCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ConnectVoiceHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void ConnectVoice(long lobbyId, ConnectVoiceHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ConnectVoice(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), ConnectVoiceCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void DisconnectVoiceCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (DisconnectVoiceHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void DisconnectVoice(long lobbyId, DisconnectVoiceHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.DisconnectVoice(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DisconnectVoiceCallbackImpl);
    }

    public void ConnectNetwork(long lobbyId)
    {
        var res = Methods.ConnectNetwork(MethodsPtr, lobbyId);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public void DisconnectNetwork(long lobbyId)
    {
        var res = Methods.DisconnectNetwork(MethodsPtr, lobbyId);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public void FlushNetwork()
    {
        var res = Methods.FlushNetwork(MethodsPtr);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public void OpenNetworkChannel(long lobbyId, byte channelId, bool reliable)
    {
        var res = Methods.OpenNetworkChannel(MethodsPtr, lobbyId, channelId, reliable);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public void SendNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
    {
        var res = Methods.SendNetworkMessage(MethodsPtr, lobbyId, userId, channelId, data, data.Length);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    [MonoPInvokeCallback]
    public static void OnLobbyUpdateImpl(nint ptr, long lobbyId)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.LobbyManagerInstance.OnLobbyUpdate?.Invoke(lobbyId);
    }

    [MonoPInvokeCallback]
    public static void OnLobbyDeleteImpl(nint ptr, long lobbyId, uint reason)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.LobbyManagerInstance.OnLobbyDelete?.Invoke(lobbyId, reason);
    }

    [MonoPInvokeCallback]
    public static void OnMemberConnectImpl(nint ptr, long lobbyId, long userId)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.LobbyManagerInstance.OnMemberConnect?.Invoke(lobbyId, userId);
    }

    [MonoPInvokeCallback]
    public static void OnMemberUpdateImpl(nint ptr, long lobbyId, long userId)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.LobbyManagerInstance.OnMemberUpdate?.Invoke(lobbyId, userId);
    }

    [MonoPInvokeCallback]
    public static void OnMemberDisconnectImpl(nint ptr, long lobbyId, long userId)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.LobbyManagerInstance.OnMemberDisconnect?.Invoke(lobbyId, userId);
    }

    [MonoPInvokeCallback]
    public static void OnLobbyMessageImpl(nint ptr, long lobbyId, long userId, nint dataPtr, int dataLen)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnLobbyMessage != null)
        {
            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            d.LobbyManagerInstance.OnLobbyMessage.Invoke(lobbyId, userId, data);
        }
    }

    [MonoPInvokeCallback]
    public static void OnSpeakingImpl(nint ptr, long lobbyId, long userId, bool speaking)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.LobbyManagerInstance.OnSpeaking?.Invoke(lobbyId, userId, speaking);
    }

    [MonoPInvokeCallback]
    public static void OnNetworkMessageImpl(nint ptr, long lobbyId, long userId, byte channelId, nint dataPtr, int dataLen)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnNetworkMessage != null)
        {
            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            d.LobbyManagerInstance.OnNetworkMessage.Invoke(lobbyId, userId, channelId, data);
        }
    }
}

internal class NetworkManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void MessageHandler(nint ptr, ulong peerId, byte channelId, nint dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void RouteUpdateHandler(nint ptr, [MarshalAs(UnmanagedType.LPStr)] string routeData);

        public MessageHandler OnMessage;

        public RouteUpdateHandler OnRouteUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void GetPeerIdMethod(nint methodsPtr, ref ulong peerId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result FlushMethod(nint methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result OpenPeerMethod(nint methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result UpdatePeerMethod(nint methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result ClosePeerMethod(nint methodsPtr, ulong peerId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result OpenChannelMethod(nint methodsPtr, ulong peerId, byte channelId, bool reliable);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result CloseChannelMethod(nint methodsPtr, ulong peerId, byte channelId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SendMessageMethod(nint methodsPtr, ulong peerId, byte channelId, byte[] data, int dataLen);

        public GetPeerIdMethod GetPeerId;

        public FlushMethod Flush;

        public OpenPeerMethod OpenPeer;

        public UpdatePeerMethod UpdatePeer;

        public ClosePeerMethod ClosePeer;

        public OpenChannelMethod OpenChannel;

        public CloseChannelMethod CloseChannel;

        public SendMessageMethod SendMessage;
    }

    public delegate void MessageHandler(ulong peerId, byte channelId, byte[] data);

    public delegate void RouteUpdateHandler(string routeData);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event MessageHandler OnMessage;

    public event RouteUpdateHandler OnRouteUpdate;

    public NetworkManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnMessage = OnMessageImpl;
        events.OnRouteUpdate = OnRouteUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    /// <summary>
    /// Get the local peer ID for this process.
    /// </summary>
    public ulong GetPeerId()
    {
        ulong ret = new();
        Methods.GetPeerId(MethodsPtr, ref ret);
        return ret;
    }

    /// <summary>
    /// Send pending network messages.
    /// </summary>
    public void Flush()
    {
        var res = Methods.Flush(MethodsPtr);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    /// <summary>
    /// Open a connection to a remote peer.
    /// </summary>
    public void OpenPeer(ulong peerId, string routeData)
    {
        var res = Methods.OpenPeer(MethodsPtr, peerId, routeData);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    /// <summary>
    /// Update the route data for a connected peer.
    /// </summary>
    public void UpdatePeer(ulong peerId, string routeData)
    {
        var res = Methods.UpdatePeer(MethodsPtr, peerId, routeData);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    /// <summary>
    /// Close the connection to a remote peer.
    /// </summary>
    public void ClosePeer(ulong peerId)
    {
        var res = Methods.ClosePeer(MethodsPtr, peerId);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    /// <summary>
    /// Open a message channel to a connected peer.
    /// </summary>
    public void OpenChannel(ulong peerId, byte channelId, bool reliable)
    {
        var res = Methods.OpenChannel(MethodsPtr, peerId, channelId, reliable);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    /// <summary>
    /// Close a message channel to a connected peer.
    /// </summary>
    public void CloseChannel(ulong peerId, byte channelId)
    {
        var res = Methods.CloseChannel(MethodsPtr, peerId, channelId);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    /// <summary>
    /// Send a message to a connected peer over an opened message channel.
    /// </summary>
    public void SendMessage(ulong peerId, byte channelId, byte[] data)
    {
        var res = Methods.SendMessage(MethodsPtr, peerId, channelId, data, data.Length);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    [MonoPInvokeCallback]
    public static void OnMessageImpl(nint ptr, ulong peerId, byte channelId, nint dataPtr, int dataLen)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        if (d.NetworkManagerInstance.OnMessage != null)
        {
            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            d.NetworkManagerInstance.OnMessage.Invoke(peerId, channelId, data);
        }
    }

    [MonoPInvokeCallback]
    public static void OnRouteUpdateImpl(nint ptr, string routeData)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.NetworkManagerInstance.OnRouteUpdate?.Invoke(routeData);
    }
}

internal class OverlayManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ToggleHandler(nint ptr, bool locked);

        public ToggleHandler OnToggle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void IsEnabledMethod(nint methodsPtr, ref bool enabled);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void IsLockedMethod(nint methodsPtr, ref bool locked);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetLockedCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetLockedMethod(nint methodsPtr, bool locked, nint callbackData, SetLockedCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OpenActivityInviteCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OpenActivityInviteMethod(nint methodsPtr, ActivityActionType type, nint callbackData, OpenActivityInviteCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OpenGuildInviteCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OpenGuildInviteMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string code, nint callbackData, OpenGuildInviteCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OpenVoiceSettingsCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OpenVoiceSettingsMethod(nint methodsPtr, nint callbackData, OpenVoiceSettingsCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result InitDrawingDxgiMethod(nint methodsPtr, nint swapchain, bool useMessageForwarding);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void OnPresentMethod(nint methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ForwardMessageMethod(nint methodsPtr, nint message);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void KeyEventMethod(nint methodsPtr, bool down, [MarshalAs(UnmanagedType.LPStr)] string keyCode, KeyVariant variant);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CharEventMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string character);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void MouseButtonEventMethod(nint methodsPtr, byte down, int clickCount, MouseButton which, int x, int y);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void MouseMotionEventMethod(nint methodsPtr, int x, int y);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ImeCommitTextMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string text);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ImeSetCompositionMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string text, ref ImeUnderline underlines, int from, int to);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ImeCancelCompositionMethod(nint methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetImeCompositionRangeCallbackCallback(nint ptr, int from, int to, ref Rect bounds);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetImeCompositionRangeCallbackMethod(nint methodsPtr, nint callbackData, SetImeCompositionRangeCallbackCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetImeSelectionBoundsCallbackCallback(nint ptr, Rect anchor, Rect focus, bool isAnchorFirst);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetImeSelectionBoundsCallbackMethod(nint methodsPtr, nint callbackData, SetImeSelectionBoundsCallbackCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool IsPointInsideClickZoneMethod(nint methodsPtr, int x, int y);

        public IsEnabledMethod IsEnabled;

        public IsLockedMethod IsLocked;

        public SetLockedMethod SetLocked;

        public OpenActivityInviteMethod OpenActivityInvite;

        public OpenGuildInviteMethod OpenGuildInvite;

        public OpenVoiceSettingsMethod OpenVoiceSettings;

        public InitDrawingDxgiMethod InitDrawingDxgi;

        public OnPresentMethod OnPresent;

        public ForwardMessageMethod ForwardMessage;

        public KeyEventMethod KeyEvent;

        public CharEventMethod CharEvent;

        public MouseButtonEventMethod MouseButtonEvent;

        public MouseMotionEventMethod MouseMotionEvent;

        public ImeCommitTextMethod ImeCommitText;

        public ImeSetCompositionMethod ImeSetComposition;

        public ImeCancelCompositionMethod ImeCancelComposition;

        public SetImeCompositionRangeCallbackMethod SetImeCompositionRangeCallback;

        public SetImeSelectionBoundsCallbackMethod SetImeSelectionBoundsCallback;

        public IsPointInsideClickZoneMethod IsPointInsideClickZone;
    }

    public delegate void SetLockedHandler(Result result);

    public delegate void OpenActivityInviteHandler(Result result);

    public delegate void OpenGuildInviteHandler(Result result);

    public delegate void OpenVoiceSettingsHandler(Result result);

    public delegate void SetImeCompositionRangeCallbackHandler(int from, int to, ref Rect bounds);

    public delegate void SetImeSelectionBoundsCallbackHandler(Rect anchor, Rect focus, bool isAnchorFirst);

    public delegate void ToggleHandler(bool locked);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event ToggleHandler OnToggle;

    public OverlayManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnToggle = OnToggleImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public bool IsEnabled()
    {
        bool ret = new();
        Methods.IsEnabled(MethodsPtr, ref ret);
        return ret;
    }

    public bool IsLocked()
    {
        bool ret = new();
        Methods.IsLocked(MethodsPtr, ref ret);
        return ret;
    }

    [MonoPInvokeCallback]
    public static void SetLockedCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SetLockedHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SetLocked(bool locked, SetLockedHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SetLocked(MethodsPtr, locked, GCHandle.ToIntPtr(wrapped), SetLockedCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void OpenActivityInviteCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (OpenActivityInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void OpenActivityInvite(ActivityActionType type, OpenActivityInviteHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.OpenActivityInvite(MethodsPtr, type, GCHandle.ToIntPtr(wrapped), OpenActivityInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void OpenGuildInviteCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (OpenGuildInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void OpenGuildInvite(string code, OpenGuildInviteHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.OpenGuildInvite(MethodsPtr, code, GCHandle.ToIntPtr(wrapped), OpenGuildInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void OpenVoiceSettingsCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (OpenVoiceSettingsHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void OpenVoiceSettings(OpenVoiceSettingsHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.OpenVoiceSettings(MethodsPtr, GCHandle.ToIntPtr(wrapped), OpenVoiceSettingsCallbackImpl);
    }

    public void InitDrawingDxgi(nint swapchain, bool useMessageForwarding)
    {
        var res = Methods.InitDrawingDxgi(MethodsPtr, swapchain, useMessageForwarding);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public void OnPresent() => Methods.OnPresent(MethodsPtr);

    public void ForwardMessage(nint message) => Methods.ForwardMessage(MethodsPtr, message);

    public void KeyEvent(bool down, string keyCode, KeyVariant variant) => Methods.KeyEvent(MethodsPtr, down, keyCode, variant);

    public void CharEvent(string character) => Methods.CharEvent(MethodsPtr, character);

    public void MouseButtonEvent(byte down, int clickCount, MouseButton which, int x, int y) => Methods.MouseButtonEvent(MethodsPtr, down, clickCount, which, x, y);

    public void MouseMotionEvent(int x, int y) => Methods.MouseMotionEvent(MethodsPtr, x, y);

    public void ImeCommitText(string text) => Methods.ImeCommitText(MethodsPtr, text);

    public void ImeSetComposition(string text, ImeUnderline underlines, int from, int to) => Methods.ImeSetComposition(MethodsPtr, text, ref underlines, from, to);

    public void ImeCancelComposition() => Methods.ImeCancelComposition(MethodsPtr);

    [MonoPInvokeCallback]
    public static void SetImeCompositionRangeCallbackCallbackImpl(nint ptr, int from, int to, ref Rect bounds)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SetImeCompositionRangeCallbackHandler)h.Target;
        h.Free();
        callback(from, to, ref bounds);
    }

    public void SetImeCompositionRangeCallback(SetImeCompositionRangeCallbackHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SetImeCompositionRangeCallback(MethodsPtr, GCHandle.ToIntPtr(wrapped), SetImeCompositionRangeCallbackCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void SetImeSelectionBoundsCallbackCallbackImpl(nint ptr, Rect anchor, Rect focus, bool isAnchorFirst)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SetImeSelectionBoundsCallbackHandler)h.Target;
        h.Free();
        callback(anchor, focus, isAnchorFirst);
    }

    public void SetImeSelectionBoundsCallback(SetImeSelectionBoundsCallbackHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SetImeSelectionBoundsCallback(MethodsPtr, GCHandle.ToIntPtr(wrapped), SetImeSelectionBoundsCallbackCallbackImpl);
    }

    public bool IsPointInsideClickZone(int x, int y) => Methods.IsPointInsideClickZone(MethodsPtr, x, y);

    [MonoPInvokeCallback]
    public static void OnToggleImpl(nint ptr, bool locked)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.OverlayManagerInstance.OnToggle?.Invoke(locked);
    }
}

internal class StorageManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result ReadMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen, ref uint read);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ReadAsyncCallback(nint ptr, Result result, nint dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ReadAsyncMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, nint callbackData, ReadAsyncCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ReadAsyncPartialCallback(nint ptr, Result result, nint dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void ReadAsyncPartialMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ulong offset, ulong length, nint callbackData, ReadAsyncPartialCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result WriteMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void WriteAsyncCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void WriteAsyncMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen, nint callbackData, WriteAsyncCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result DeleteMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result ExistsMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ref bool exists);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CountMethod(nint methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result StatMethod(nint methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ref FileStat stat);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result StatAtMethod(nint methodsPtr, int index, ref FileStat stat);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetPathMethod(nint methodsPtr, StringBuilder path);

        public ReadMethod Read;

        public ReadAsyncMethod ReadAsync;

        public ReadAsyncPartialMethod ReadAsyncPartial;

        public WriteMethod Write;

        public WriteAsyncMethod WriteAsync;

        public DeleteMethod Delete;

        public ExistsMethod Exists;

        public CountMethod Count;

        public StatMethod Stat;

        public StatAtMethod StatAt;

        public GetPathMethod GetPath;
    }

    public delegate void ReadAsyncHandler(Result result, byte[] data);

    public delegate void ReadAsyncPartialHandler(Result result, byte[] data);

    public delegate void WriteAsyncHandler(Result result);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public StorageManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public IEnumerable<FileStat> Files()
    {
        int fileCount = Count();
        var files = new List<FileStat>();
        for (int i = 0; i < fileCount; i++)
        {
            files.Add(StatAt(i));
        }

        return files;
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events) => Marshal.StructureToPtr(events, eventsPtr, false);

    public uint Read(string name, byte[] data)
    {
        uint ret = new();
        var res = Methods.Read(MethodsPtr, name, data, data.Length, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void ReadAsyncCallbackImpl(nint ptr, Result result, nint dataPtr, int dataLen)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ReadAsyncHandler)h.Target;
        h.Free();
        byte[] data = new byte[dataLen];
        Marshal.Copy(dataPtr, data, 0, dataLen);
        callback(result, data);
    }

    public void ReadAsync(string name, ReadAsyncHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ReadAsync(MethodsPtr, name, GCHandle.ToIntPtr(wrapped), ReadAsyncCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void ReadAsyncPartialCallbackImpl(nint ptr, Result result, nint dataPtr, int dataLen)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (ReadAsyncPartialHandler)h.Target;
        h.Free();
        byte[] data = new byte[dataLen];
        Marshal.Copy(dataPtr, data, 0, dataLen);
        callback(result, data);
    }

    public void ReadAsyncPartial(string name, ulong offset, ulong length, ReadAsyncPartialHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.ReadAsyncPartial(MethodsPtr, name, offset, length, GCHandle.ToIntPtr(wrapped), ReadAsyncPartialCallbackImpl);
    }

    public void Write(string name, byte[] data)
    {
        var res = Methods.Write(MethodsPtr, name, data, data.Length);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    [MonoPInvokeCallback]
    public static void WriteAsyncCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (WriteAsyncHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void WriteAsync(string name, byte[] data, WriteAsyncHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.WriteAsync(MethodsPtr, name, data, data.Length, GCHandle.ToIntPtr(wrapped), WriteAsyncCallbackImpl);
    }

    public void Delete(string name)
    {
        var res = Methods.Delete(MethodsPtr, name);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public bool Exists(string name)
    {
        bool ret = new();
        var res = Methods.Exists(MethodsPtr, name, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public int Count()
    {
        int ret = new();
        Methods.Count(MethodsPtr, ref ret);
        return ret;
    }

    public FileStat Stat(string name)
    {
        var ret = new FileStat();
        var res = Methods.Stat(MethodsPtr, name, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public FileStat StatAt(int index)
    {
        var ret = new FileStat();
        var res = Methods.StatAt(MethodsPtr, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public string GetPath()
    {
        var ret = new StringBuilder(4096);
        var res = Methods.GetPath(MethodsPtr, ret);
        return res != Result.Ok ? throw new ResultException(res) : ret.ToString();
    }
}

internal class StoreManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void EntitlementCreateHandler(nint ptr, ref Entitlement entitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void EntitlementDeleteHandler(nint ptr, ref Entitlement entitlement);

        public EntitlementCreateHandler OnEntitlementCreate;

        public EntitlementDeleteHandler OnEntitlementDelete;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchSkusCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchSkusMethod(nint methodsPtr, nint callbackData, FetchSkusCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CountSkusMethod(nint methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetSkuMethod(nint methodsPtr, long skuId, ref Sku sku);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetSkuAtMethod(nint methodsPtr, int index, ref Sku sku);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchEntitlementsCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchEntitlementsMethod(nint methodsPtr, nint callbackData, FetchEntitlementsCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CountEntitlementsMethod(nint methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetEntitlementMethod(nint methodsPtr, long entitlementId, ref Entitlement entitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetEntitlementAtMethod(nint methodsPtr, int index, ref Entitlement entitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result HasSkuEntitlementMethod(nint methodsPtr, long skuId, ref bool hasEntitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void StartPurchaseCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void StartPurchaseMethod(nint methodsPtr, long skuId, nint callbackData, StartPurchaseCallback callback);

        public FetchSkusMethod FetchSkus;

        public CountSkusMethod CountSkus;

        public GetSkuMethod GetSku;

        public GetSkuAtMethod GetSkuAt;

        public FetchEntitlementsMethod FetchEntitlements;

        public CountEntitlementsMethod CountEntitlements;

        public GetEntitlementMethod GetEntitlement;

        public GetEntitlementAtMethod GetEntitlementAt;

        public HasSkuEntitlementMethod HasSkuEntitlement;

        public StartPurchaseMethod StartPurchase;
    }

    public delegate void FetchSkusHandler(Result result);

    public delegate void FetchEntitlementsHandler(Result result);

    public delegate void StartPurchaseHandler(Result result);

    public delegate void EntitlementCreateHandler(ref Entitlement entitlement);

    public delegate void EntitlementDeleteHandler(ref Entitlement entitlement);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event EntitlementCreateHandler OnEntitlementCreate;

    public event EntitlementDeleteHandler OnEntitlementDelete;

    public StoreManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public IEnumerable<Entitlement> GetEntitlements()
    {
        int count = CountEntitlements();
        var entitlements = new List<Entitlement>();
        for (int i = 0; i < count; i++)
        {
            entitlements.Add(GetEntitlementAt(i));
        }

        return entitlements;
    }

    public IEnumerable<Sku> GetSkus()
    {
        int count = CountSkus();
        var skus = new List<Sku>();
        for (int i = 0; i < count; i++)
        {
            skus.Add(GetSkuAt(i));
        }

        return skus;
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnEntitlementCreate = OnEntitlementCreateImpl;
        events.OnEntitlementDelete = OnEntitlementDeleteImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    public static void FetchSkusCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (FetchSkusHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void FetchSkus(FetchSkusHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.FetchSkus(MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchSkusCallbackImpl);
    }

    public int CountSkus()
    {
        int ret = new();
        Methods.CountSkus(MethodsPtr, ref ret);
        return ret;
    }

    public Sku GetSku(long skuId)
    {
        var ret = new Sku();
        var res = Methods.GetSku(MethodsPtr, skuId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public Sku GetSkuAt(int index)
    {
        var ret = new Sku();
        var res = Methods.GetSkuAt(MethodsPtr, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void FetchEntitlementsCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (FetchEntitlementsHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void FetchEntitlements(FetchEntitlementsHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.FetchEntitlements(MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchEntitlementsCallbackImpl);
    }

    public int CountEntitlements()
    {
        int ret = new();
        Methods.CountEntitlements(MethodsPtr, ref ret);
        return ret;
    }

    public Entitlement GetEntitlement(long entitlementId)
    {
        var ret = new Entitlement();
        var res = Methods.GetEntitlement(MethodsPtr, entitlementId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public Entitlement GetEntitlementAt(int index)
    {
        var ret = new Entitlement();
        var res = Methods.GetEntitlementAt(MethodsPtr, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public bool HasSkuEntitlement(long skuId)
    {
        bool ret = new();
        var res = Methods.HasSkuEntitlement(MethodsPtr, skuId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void StartPurchaseCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (StartPurchaseHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void StartPurchase(long skuId, StartPurchaseHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.StartPurchase(MethodsPtr, skuId, GCHandle.ToIntPtr(wrapped), StartPurchaseCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void OnEntitlementCreateImpl(nint ptr, ref Entitlement entitlement)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.StoreManagerInstance.OnEntitlementCreate?.Invoke(ref entitlement);
    }

    [MonoPInvokeCallback]
    public static void OnEntitlementDeleteImpl(nint ptr, ref Entitlement entitlement)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.StoreManagerInstance.OnEntitlementDelete?.Invoke(ref entitlement);
    }
}

internal class VoiceManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SettingsUpdateHandler(nint ptr);

        public SettingsUpdateHandler OnSettingsUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetInputModeMethod(nint methodsPtr, ref InputMode inputMode);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetInputModeCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetInputModeMethod(nint methodsPtr, InputMode inputMode, nint callbackData, SetInputModeCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result IsSelfMuteMethod(nint methodsPtr, ref bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetSelfMuteMethod(nint methodsPtr, bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result IsSelfDeafMethod(nint methodsPtr, ref bool deaf);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetSelfDeafMethod(nint methodsPtr, bool deaf);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result IsLocalMuteMethod(nint methodsPtr, long userId, ref bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetLocalMuteMethod(nint methodsPtr, long userId, bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetLocalVolumeMethod(nint methodsPtr, long userId, ref byte volume);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result SetLocalVolumeMethod(nint methodsPtr, long userId, byte volume);

        public GetInputModeMethod GetInputMode;

        public SetInputModeMethod SetInputMode;

        public IsSelfMuteMethod IsSelfMute;

        public SetSelfMuteMethod SetSelfMute;

        public IsSelfDeafMethod IsSelfDeaf;

        public SetSelfDeafMethod SetSelfDeaf;

        public IsLocalMuteMethod IsLocalMute;

        public SetLocalMuteMethod SetLocalMute;

        public GetLocalVolumeMethod GetLocalVolume;

        public SetLocalVolumeMethod SetLocalVolume;
    }

    public delegate void SetInputModeHandler(Result result);

    public delegate void SettingsUpdateHandler();

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event SettingsUpdateHandler OnSettingsUpdate;

    public VoiceManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnSettingsUpdate = OnSettingsUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public InputMode GetInputMode()
    {
        var ret = new InputMode();
        var res = Methods.GetInputMode(MethodsPtr, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void SetInputModeCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SetInputModeHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SetInputMode(InputMode inputMode, SetInputModeHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SetInputMode(MethodsPtr, inputMode, GCHandle.ToIntPtr(wrapped), SetInputModeCallbackImpl);
    }

    public bool IsSelfMute()
    {
        bool ret = new();
        var res = Methods.IsSelfMute(MethodsPtr, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public void SetSelfMute(bool mute)
    {
        var res = Methods.SetSelfMute(MethodsPtr, mute);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public bool IsSelfDeaf()
    {
        bool ret = new();
        var res = Methods.IsSelfDeaf(MethodsPtr, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public void SetSelfDeaf(bool deaf)
    {
        var res = Methods.SetSelfDeaf(MethodsPtr, deaf);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public bool IsLocalMute(long userId)
    {
        bool ret = new();
        var res = Methods.IsLocalMute(MethodsPtr, userId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public void SetLocalMute(long userId, bool mute)
    {
        var res = Methods.SetLocalMute(MethodsPtr, userId, mute);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    public byte GetLocalVolume(long userId)
    {
        byte ret = new();
        var res = Methods.GetLocalVolume(MethodsPtr, userId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public void SetLocalVolume(long userId, byte volume)
    {
        var res = Methods.SetLocalVolume(MethodsPtr, userId, volume);
        if (res != Result.Ok)
        {
            throw new ResultException(res);
        }
    }

    [MonoPInvokeCallback]
    public static void OnSettingsUpdateImpl(nint ptr)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.VoiceManagerInstance.OnSettingsUpdate?.Invoke();
    }
}

internal class AchievementManager
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FFIEvents
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void UserAchievementUpdateHandler(nint ptr, ref UserAchievement userAchievement);

        public UserAchievementUpdateHandler OnUserAchievementUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FFIMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetUserAchievementCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void SetUserAchievementMethod(nint methodsPtr, long achievementId, byte percentComplete, nint callbackData, SetUserAchievementCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchUserAchievementsCallback(nint ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void FetchUserAchievementsMethod(nint methodsPtr, nint callbackData, FetchUserAchievementsCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void CountUserAchievementsMethod(nint methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetUserAchievementMethod(nint methodsPtr, long userAchievementId, ref UserAchievement userAchievement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate Result GetUserAchievementAtMethod(nint methodsPtr, int index, ref UserAchievement userAchievement);

        public SetUserAchievementMethod SetUserAchievement;

        public FetchUserAchievementsMethod FetchUserAchievements;

        public CountUserAchievementsMethod CountUserAchievements;

        public GetUserAchievementMethod GetUserAchievement;

        public GetUserAchievementAtMethod GetUserAchievementAt;
    }

    public delegate void SetUserAchievementHandler(Result result);

    public delegate void FetchUserAchievementsHandler(Result result);

    public delegate void UserAchievementUpdateHandler(ref UserAchievement userAchievement);

    public readonly nint MethodsPtr;

    public object MethodsStructure;

    public FFIMethods Methods
    {
        get
        {
            MethodsStructure ??= Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)MethodsStructure;
        }
    }

    public event UserAchievementUpdateHandler OnUserAchievementUpdate;

    public AchievementManager(nint ptr, nint eventsPtr, ref FFIEvents events)
    {
        if (eventsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }

        InitEvents(eventsPtr, ref events);
        MethodsPtr = ptr;
        if (MethodsPtr == nint.Zero)
        {
            throw new ResultException(Result.InternalError);
        }
    }

    public void InitEvents(nint eventsPtr, ref FFIEvents events)
    {
        events.OnUserAchievementUpdate = OnUserAchievementUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    public static void SetUserAchievementCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (SetUserAchievementHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SetUserAchievement(long achievementId, byte percentComplete, SetUserAchievementHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.SetUserAchievement(MethodsPtr, achievementId, percentComplete, GCHandle.ToIntPtr(wrapped), SetUserAchievementCallbackImpl);
    }

    [MonoPInvokeCallback]
    public static void FetchUserAchievementsCallbackImpl(nint ptr, Result result)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var callback = (FetchUserAchievementsHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void FetchUserAchievements(FetchUserAchievementsHandler callback)
    {
        var wrapped = GCHandle.Alloc(callback);
        Methods.FetchUserAchievements(MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchUserAchievementsCallbackImpl);
    }

    public int CountUserAchievements()
    {
        int ret = new();
        Methods.CountUserAchievements(MethodsPtr, ref ret);
        return ret;
    }

    public UserAchievement GetUserAchievement(long userAchievementId)
    {
        var ret = new UserAchievement();
        var res = Methods.GetUserAchievement(MethodsPtr, userAchievementId, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    public UserAchievement GetUserAchievementAt(int index)
    {
        var ret = new UserAchievement();
        var res = Methods.GetUserAchievementAt(MethodsPtr, index, ref ret);
        return res != Result.Ok ? throw new ResultException(res) : ret;
    }

    [MonoPInvokeCallback]
    public static void OnUserAchievementUpdateImpl(nint ptr, ref UserAchievement userAchievement)
    {
        var h = GCHandle.FromIntPtr(ptr);
        var d = (Discord)h.Target;
        d.AchievementManagerInstance.OnUserAchievementUpdate?.Invoke(ref userAchievement);
    }
}