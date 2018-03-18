//
//  SensorsDataUnityBridge.h
//  SensorsAnalyticsSDK
//
//  Created by zhanghaoyang on 16/12/26.
//  Copyright © 2016年 SensorsData. All rights reserved.
//

#import "SensorsAnalyticsSDK.h"

@interface SensorsDataUnityBridge : NSObject
+(void)InitSensorsData:(NSString *)SERVERURL sec:(NSString *)CONFIGURL thr:(BOOL)isdebug;
+(void)TrackEvent:(NSString *)EventName sec:(NSString *)JsonString;
//+(void)TrackEvent:(NSString *)EventName;
+(void)SetUserId:(NSString *)UserId;
+(void)SetGameInfo:(NSString *)channel sec:(NSString *)codename;
+(void)SetProfile:(NSString *)name va:(NSString *)value;
+(void)SetDicProfile:(NSString *)JsonString;
+(void)TrackInstallation;
@end
