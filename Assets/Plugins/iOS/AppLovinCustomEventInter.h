//
// AppLovin <--> AdMob Network Adaptors
//

#if __has_include(<AppLovinSDK/AppLovinSDK.h>)
    #import <AppLovinSDK/AppLovinSDK.h>
#else
    #import "ALAdService.h"
    #import "ALInterstitialAd.h"
#endif

@import GoogleMobileAds;
@import UIKit;

@interface AppLovinCustomEventInter : NSObject <GADCustomEventInterstitial, ALAdLoadDelegate, ALAdDisplayDelegate>

@property (strong, atomic) ALAd* appLovinAd;

@end
