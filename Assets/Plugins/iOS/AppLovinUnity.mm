//
//  AppLovinUnity.mm
//  sdk
//

#import "ALSdk.h"
#import "ALAdView.h"
#import "ALInterstitialAd.h"
#import "ALSdkSettings.h"
#import "ALAdDelegateWrapper.h"
#import "ALIncentivizedInterstitialAd.h"
#import "ALInterstitialCache.h"
#import "ALAdType.h"
#import "ALManagedLoadDelegate.h"
#import "ALEventTypes.h"

UIView* UnityGetGLView();

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
    static const NSString * UNITY_PLUGIN_VERSION = @"4.4.0";
    static const NSString * UNITY_BUILD_NUMBER = @"40400";
    
    static const CGFloat POSITION_CENTER = -10000;
    static const CGFloat POSITION_LEFT = -20000;
    static const CGFloat POSITION_RIGHT = -30000;
    static const CGFloat POSITION_TOP = -40000;
    static const CGFloat POSITION_BOTTOM = -50000;

    static NSString * const SERIALIZED_KEY_VALUE_PAIR_SEPARATOR = [NSString stringWithFormat: @"%c", 28];
    static NSString * const SERIALIZED_KEY_VALUE_SEPARATOR = [NSString stringWithFormat: @"%c", 29];

    static CGFloat adX;
    static CGFloat adY;

    static ALInterstitialCache* interCache;

    static ALAdView *adView;
    static ALAdDelegateWrapper* delegateWrapper;
    static ALIncentivizedInterstitialAd* incentInter;

    /**
     *  For internal use only
     */

    void maybeInitializeDelegateWrapper()
    {
        if(!delegateWrapper) { delegateWrapper = [[ALAdDelegateWrapper alloc] init]; }
        if(!interCache) { interCache = [ALInterstitialCache shared]; }

        interCache.wrapperToNotify = delegateWrapper;
    }

    ALAdView *SharedAdview()
    {
        if (!adView)
        {
            [[ALSdk shared] setPluginVersion:[NSString stringWithFormat:@"unity-%@", UNITY_PLUGIN_VERSION]];
            adView = [[ALAdView alloc] initWithSize: [ALAdSize sizeBanner]];

            maybeInitializeDelegateWrapper();
            [adView setAdLoadDelegate: [ALManagedLoadDelegate sharedDelegateForSize: [ALAdSize sizeBanner] type: [ALAdType typeRegular] wrapper: delegateWrapper]];
            [adView setAdDisplayDelegate: delegateWrapper];
            [adView setAdEventDelegate: delegateWrapper];
        }

        return adView;
    }

    /**
     * Initialize the AppLovin SDK manually
     */
    void _AppLovinInitializeSdk()
    {
        [[ALSdk shared] initializeSdk];
        [[ALSdk shared] setPluginVersion: [NSString stringWithFormat:@"unity-%@", UNITY_PLUGIN_VERSION]];
        maybeInitializeDelegateWrapper();
    }
    
    /**
     *  Show AppLovin Banner Ad
     */
    void _AppLovinShowAd()
    {
        [SharedAdview() setHidden:false];
        
        [SharedAdview() loadNextAd];
        [UnityGetGLView() addSubview:SharedAdview()];
    }

    /**
     *  Hide AppLovin Banner Ad
     */
    void _AppLovinHideAd()
    {
        [SharedAdview() setHidden:true];
    }

    /**
     *  Show AppLovin Interstitial Ad
     */
    void _AppLovinShowInterstitial(const char * placement)
    {
        maybeInitializeDelegateWrapper();

        [ALInterstitialAd shared].adDisplayDelegate = delegateWrapper;
        [ALInterstitialAd shared].adLoadDelegate = [ALManagedLoadDelegate sharedDelegateForSize: [ALAdSize sizeInterstitial] type: [ALAdType typeRegular] wrapper: delegateWrapper];
        [ALInterstitialAd shared].adVideoPlaybackDelegate = delegateWrapper;

        ALAd* lastAd = [ALInterstitialCache shared].lastAd;
        ALInterstitialAd* shared = [ALInterstitialAd shared];

        NSString * placementName = nil;

        if (placement != NULL)
        {
            placementName = [NSString stringWithCString: placement encoding: NSStringEncodingConversionAllowLossy];
        }

        if( lastAd )
        {
            [shared showOver: [[UIApplication sharedApplication] keyWindow] placement: placementName andRender: lastAd];
        }
        else
        {
            [ALInterstitialAd showOver:[[UIApplication sharedApplication] keyWindow] placement: placementName];
        }
    }


    /**
     *  For internal use only
     */
    CGFloat getAvailableScreenWidth()
    {
        CGRect screenBounds = [[UIScreen mainScreen] applicationFrame];

        UIInterfaceOrientation orientation = [[UIApplication sharedApplication] statusBarOrientation];

        CGFloat width = screenBounds.size.width;

        // Don't trust the system
        if ((UIInterfaceOrientationIsLandscape(orientation) && screenBounds.size.height > screenBounds.size.width) || (UIInterfaceOrientationIsPortrait(orientation) && screenBounds.size.width > screenBounds.size.height))
        {
            width = screenBounds.size.height;
        }

        return width;
    }

    /**
     *  For internal use only
     */
    CGFloat getAvailableScreenHeight()
    {
        CGRect screenBounds = [[UIScreen mainScreen] applicationFrame];

        UIInterfaceOrientation orientation = [[UIApplication sharedApplication] statusBarOrientation];

        CGFloat height = screenBounds.size.height;

        if ((UIInterfaceOrientationIsLandscape(orientation) && screenBounds.size.height > screenBounds.size.width) || (UIInterfaceOrientationIsPortrait(orientation) && screenBounds.size.width > screenBounds.size.height))
        {
            height = screenBounds.size.width;
        }

        return height;
    }

    /**
     *  For internal use only
     */
    void updateAdPosition()
    {
        CGRect newRect = SharedAdview().frame;

        if (adX == POSITION_CENTER)
        {
            newRect.origin.x = getAvailableScreenWidth() / 2 - newRect.size.width / 2;
        }
        else if (adX == POSITION_LEFT)
        {
            newRect.origin.x = 0;
        }
        else if (adX == POSITION_RIGHT)
        {
            newRect.origin.x = getAvailableScreenWidth() - newRect.size.width;
        }
        else
        {
            newRect.origin.x = adX;
        }

        if (adY == POSITION_TOP)
        {
            newRect.origin.y = 0;
        }
        else if (adY == POSITION_BOTTOM)
        {
            newRect.origin.y = getAvailableScreenHeight() - newRect.size.height;
        }
        else
        {
            newRect.origin.y = adY;
        }

        SharedAdview().frame = newRect;
    }

    /**
     * Set the position of the banner ad
     *
     * @param float x   Horizontal position of the ad in dp or constant (POSITION_LEFT, POSITION_CENTER, POSITION_RIGHT)
     * @param float y   Veritcal position of the ad in dp or constant (POSITION_TOP, POSITION_BOTTOM)
     */
    void _AppLovinSetAdPosition(float x, float y)
    {
        adX = x;
        adY = y;

        updateAdPosition();
    }

    /**
     * Set the width of the banner ad
     *
     * @param float width   Width of the ad in dp
     */
    void _AppLovinSetAdWidth(CGFloat width)
    {
        CGRect newRect = [SharedAdview() frame];

        newRect.size.width = width;

        SharedAdview().frame = newRect;

        updateAdPosition();
    }

    /**
     * Set the AppLovin SDK key for the application
     *
     * @param string sdkKey    The SDK key for the application
     */
    void _AppLovinSetSdkKey(const char * sdkKey)
    {
        if (!sdkKey) { return; };

        NSString * sdkKeyStr = [NSString stringWithUTF8String:sdkKey];

        NSDictionary* infoDict = [[NSBundle mainBundle] infoDictionary];
        [infoDict setValue:sdkKeyStr forKey:@"AppLovinSdkKey"];
    }

    void _AppLovinSetVerboseLoggingOn(const char * verboseLogging)
    {
        NSString* verboseLoggingStr = [NSString stringWithUTF8String: verboseLogging];
        [[ALSdk shared] settings].isVerboseLogging = [verboseLoggingStr boolValue];
    }

    void _AppLovinSetMuted(const char * muted)
    {
        NSString *mutedStr = [NSString stringWithUTF8String: muted];
        [[ALSdk shared] settings].muted = [mutedStr boolValue];
    }
    
    bool _AppLovinIsMuted()
    {
        return [[ALSdk shared] settings].muted;
    }
    
    void _AppLovinSetTestAdsEnabled(const char * enabled)
    {
        NSString *enabledStr = [NSString stringWithUTF8String: enabled];
        [[ALSdk shared] settings].isTestAdsEnabled = [enabledStr boolValue];
    }
    
    bool _AppLovinIsTestAdsEnabled()
    {
        return [[ALSdk shared] settings].isTestAdsEnabled;
    }
    
    void _AppLovinPreloadInterstitial()
    {
        [[[ALSdk shared] adService] loadNextAd: [ALAdSize sizeInterstitial] andNotify: [ALManagedLoadDelegate sharedDelegateForSize: [ALAdSize sizeInterstitial] type: [ALAdType typeRegular] wrapper: interCache]];
    }

    bool _AppLovinHasPreloadedInterstitial()
    {
        return ([interCache lastAd] != nil) || [[[ALSdk shared] adService] hasPreloadedAdOfSize: [ALAdSize sizeInterstitial]];
    }

    bool _AppLovinIsInterstitialShowing()
    {
        return ([delegateWrapper isInterstitialShowing] ? true : false);
    }
    
    void _AppLovinSetUnityAdListener(const char* gameObjectToNotify)
    {
        maybeInitializeDelegateWrapper();
        delegateWrapper.gameObjectToNotify = [NSString stringWithCString: gameObjectToNotify encoding: NSUTF8StringEncoding];
    }

    void _AppLovinLoadIncentInterstitial()
    {
        maybeInitializeDelegateWrapper();

        if(!incentInter)
        {
            incentInter = [[ALIncentivizedInterstitialAd alloc] initWithSdk: [ALSdk shared]];
        }

        [incentInter preloadAndNotify: [ALManagedLoadDelegate sharedDelegateForSize: [ALAdSize sizeInterstitial] type: [ALAdType typeIncentivized] wrapper: delegateWrapper]];
    }

    void _AppLovinShowIncentInterstitial(const char * placement)
    {
        maybeInitializeDelegateWrapper();

        NSString * placementName = nil;

        if (placement != NULL)
        {
            placementName = [NSString stringWithCString: placement encoding: NSStringEncodingConversionAllowLossy];
        }

        if(incentInter)
        {
            incentInter.adDisplayDelegate = delegateWrapper;
            incentInter.adVideoPlaybackDelegate = delegateWrapper;
            [incentInter showOver: [[UIApplication sharedApplication] keyWindow] placement: placementName andNotify: delegateWrapper];
        }
    }

    void _AppLovinSetIncentivizedUserName(const char * username)
    {
        [ALIncentivizedInterstitialAd setUserIdentifier: [NSString stringWithCString: username encoding: NSStringEncodingConversionAllowLossy]];
    }

    bool _AppLovinIsIncentReady()
    {
        maybeInitializeDelegateWrapper();
        return delegateWrapper.isIncentReady ? true : false;
    }

    bool _AppLovinIsCurrentInterstitialVideo()
    {
        maybeInitializeDelegateWrapper();

        if(interCache.lastAd)
        {
            return interCache.lastAd.videoAd ? true : false;
        }
        return false;
    }

    NSDictionary* deserializeParameters(const char * serializedParameters)
    {
        if (serializedParameters != NULL)
        {
            NSString* objcSerializedParameters = [NSString stringWithCString: serializedParameters encoding: NSUTF8StringEncoding];
            NSArray* keyValuePairs = [objcSerializedParameters componentsSeparatedByString: SERIALIZED_KEY_VALUE_PAIR_SEPARATOR];
            NSMutableDictionary* deserializedParameters = [NSMutableDictionary dictionary];

            for (NSString* keyValuePair in keyValuePairs)
            {
                NSArray* splitPair = [keyValuePair componentsSeparatedByString: SERIALIZED_KEY_VALUE_SEPARATOR];

                if ([splitPair count] > 1)
                {
                    NSString* key = [splitPair objectAtIndex: 0];
                    NSString* value = [splitPair objectAtIndex: 1];

                    if (key && value)
                    {
                        [deserializedParameters setObject: value forKey: key];
                    }
                }
            }

            return deserializedParameters;
        }

        return [NSDictionary dictionary];
    }

    void _AppLovinTrackAnalyticEvent(const char * eventType, const char * serializedParameters)
    {
        // Some versions of Unity 5 have a bug where strings that come in via C# (e.g. the arguments here) get free()'d mid-method body.
        // Seems like a concurrency issue on their part. To avoid, we copy the strings and use the copies.

        const int eventTypeCopySize = 128;
        const int serializedParametersCopySize = 2048;

        char eventTypeCopy[eventTypeCopySize];
        char serializedParametersCopy[serializedParametersCopySize];

        size_t eventCopyLen = strlcpy(eventTypeCopy, eventType, sizeof(eventTypeCopy));
        size_t serializedParamsCopyLen = strlcpy(serializedParametersCopy, serializedParameters, sizeof(serializedParametersCopy));

        if (eventCopyLen > eventTypeCopySize)
        {
            NSLog(@"[AppLovinUnity] Event type has been truncated to %@", [NSString stringWithCString: eventTypeCopy encoding: NSUTF8StringEncoding]);
        }

        if (serializedParamsCopyLen > serializedParametersCopySize)
        {
            NSLog(@"[AppLovinUnity] Event parameters were too large and have been truncated, large key-value pairs may be dropped...");
        }

        NSDictionary* deserializedParameters = deserializeParameters(serializedParametersCopy);
        NSString* objcEventType = [NSString stringWithCString: eventTypeCopy encoding: NSUTF8StringEncoding];

        if ([objcEventType isEqualToString: kALEventTypeUserCompletedInAppPurchase])
        {
            NSString * transactionIdentifier = deserializedParameters[kALEventParameterStoreKitTransactionIdentifierKey];
            [[ALSdk shared].eventService trackInAppPurchaseWithTransactionIdentifier: transactionIdentifier parameters: deserializedParameters];
        }
        else
        {
            [[ALSdk shared].eventService trackEvent: objcEventType parameters: deserializedParameters];
        }
    }
}
