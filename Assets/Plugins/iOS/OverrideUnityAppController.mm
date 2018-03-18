#import "OverrideUnityAppController.h"
#import <UIKit/UIKit.h>
#import "SvUDIDTools.h"  

extern "C"{
    void resetNotification()
    {
        [UIApplication sharedApplication].applicationIconBadgeNumber = 0;
        [[UIApplication sharedApplication] cancelAllLocalNotifications];
    }
  
    const char* SUDID()  
    {  
        const char* recv = [[SvUDIDTools UDID] UTF8String];  
        if(nullptr == recv){  
            return nullptr;  
        }  
        char* result = (char*)malloc(strlen(recv) + 1);  
        strcpy(result, recv);  
        return result;  
    } 

    void UnitySendMessage(const char *, const char *, const char *);
}


@implementation OverrideUnityAppController

- (instancetype)init
{
    NSLog(@"initializing OverrideUnityAppController");
    self = [super init];
    if (self) {
        UnityRegisterAppDelegateListener(self);
    }
    return self;
}

- (void)didFinishLaunching:(NSNotification*)notification {
    NSLog(@"got didFinishLaunching = %@",notification.userInfo);
    if (notification.userInfo[@"url"]) {
        [self onOpenURL:notification];
    }
}

- (void)onOpenURL:(NSNotification*)notification {
    NSLog(@"got onOpenURL = %@", notification.userInfo);
    NSURL *url = notification.userInfo[@"url"];
    NSString *sourceApplication = notification.userInfo[@"sourceApplication"];
    
    if (sourceApplication == nil) {
        sourceApplication = @"";
    }
    
    if (url != nil) {
        // 给游戏传url
        NSString *str1 = [url absoluteString];
        const char *pConstChar = [str1 UTF8String];
        UnitySendMessage("GiftInit", "SentGiftURL", pConstChar);
    }
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(OverrideUnityAppController)