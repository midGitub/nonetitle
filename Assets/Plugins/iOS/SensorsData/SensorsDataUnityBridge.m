//
//  SensorsDataUnityBridge.m
//  SensorsAnalyticsSDK
//
//  Created by zhanghaoyang on 16/12/26.
//  Copyright © 2016年 SensorsData. All rights reserved.
//

#import "SensorsDataUnityBridge.h"

#if defined(__cplusplus)
extern "C"{
#endif
    void _InitSensorsData(char serverurl[], char configurl[], BOOL isdebug)
    {
        NSString *ServerUrl = [NSString stringWithUTF8String:serverurl];
        NSString *ConfigUrl = [NSString stringWithUTF8String:configurl];
        [SensorsDataUnityBridge InitSensorsData:ServerUrl sec:ConfigUrl thr:isdebug];
    }
    
    void _TrackEvent(char eventname[], char jsonstring[])
    {
        NSString *EventName = [NSString stringWithUTF8String:eventname];
        NSString *JsonString = [NSString stringWithUTF8String:jsonstring];
        [SensorsDataUnityBridge TrackEvent:EventName sec:JsonString];
    }
    
//    void _TrackEventName(char eventname[])
//    {
//        NSString *EventName = [NSString stringWithUTF8String:eventname];
//        [SensorsDataUnityBridge TrackEvent:EventName];
//    }
    
    void _SetUserId(char userid[])
    {
        NSString *Userid = [NSString stringWithUTF8String:userid];
        [SensorsDataUnityBridge SetUserId:Userid];
    }
    
    void _SetGameInfo(char channel[], char code[])
    {
        NSString *Channel = [NSString stringWithUTF8String:channel];
        NSString *Code = [NSString stringWithUTF8String:code];
        [SensorsDataUnityBridge SetGameInfo:Channel sec:Code];
    }

    void _SetProfile(char name[], char value[])
    {
        NSString *Name = [NSString stringWithUTF8String:name];
        NSString *Value = [NSString stringWithUTF8String:value];
        [SensorsDataUnityBridge SetProfile:Name va:Value];
    }

    void _SetDicProfile(char jsonstring[])
    {
        NSString *JsonString = [NSString stringWithUTF8String:jsonstring];
        [SensorsDataUnityBridge SetDicProfile:JsonString];
    }

    void _TrackInstallation()
    {
        [SensorsDataUnityBridge TrackInstallation];
    }
#if defined(__cplusplus)
}
#endif

@implementation SensorsDataUnityBridge : NSObject

static SensorsDataUnityBridge * instance;
+(instancetype)INSTANCE{
    if(instance == nil){
        instance = [[SensorsDataUnityBridge alloc] init];
    }
    return instance;
}

+(void)InitSensorsData:(NSString *)SERVERURL sec:(NSString *)CONFIGURL thr:(BOOL)isdebug
{
    // Debug 模式选项
    //   SensorsAnalyticsDebugOff - 关闭 Debug 模式
    //   SensorsAnalyticsDebugOnly - 打开 Debug 模式，校验数据，但不进行数据导入
    //   SensorsAnalyticsDebugAndTrack - 打开 Debug 模式，校验数据，并将数据导入到 Sensors Analytics 中
    // 注意！请不要在正式发布的 App 中使用 Debug 模式！
    SensorsAnalyticsDebugMode debugmode = SensorsAnalyticsDebugOff;
    if (isdebug)
    {
        debugmode = SensorsAnalyticsDebugAndTrack;
    }
    [SensorsAnalyticsSDK sharedInstanceWithServerURL:SERVERURL
                                     andConfigureURL:CONFIGURL
                                        andDebugMode:debugmode];
    
    [[SensorsAnalyticsSDK sharedInstance] enableAutoTrack];
}

+(void)TrackEvent:(NSString *)EventName sec:(NSString *)JsonString
{
    NSData* jsonData = [JsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary* dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableContainers error:nil];
    
    [[SensorsAnalyticsSDK sharedInstance] track:EventName withProperties:dic];
}

//+(void)TrackEvent:(NSString *)EventName
//{
//    [[SensorsAnalyticsSDK sharedInstance] track:EventName withProperties:nil];
//}

+(void)SetUserId:(NSString *)UserId
{
    [[SensorsAnalyticsSDK sharedInstance] login:UserId];
}

+(void)SetGameInfo:(NSString *)channel sec:(NSString *)codename
{
    [[[SensorsAnalyticsSDK sharedInstance] people] setOnce:@"Channel" to:channel];
    [[[SensorsAnalyticsSDK sharedInstance] people] setOnce:@"codename" to:codename];
}

+(void)SetProfile:(NSString *)name va:(NSString *)value
{
    [[[SensorsAnalyticsSDK sharedInstance] people] set:name to:value];
}

+(void)SetDicProfile:(NSString *)JsonString
{
    NSData* jsonData = [JsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary* dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableContainers error:nil];
    
    [[[SensorsAnalyticsSDK sharedInstance] people] set:dic];
}

+(void)TrackInstallation
{
    // 获取当前时间
    NSDate *currentTime = [NSDate date];
    // 在 App 首次启动时，追踪 App 激活事件，并记录首次激活事件
    // AppInstall 事件名称，此处仅仅是一个示例，可以根据实际需求自定义事件名称
    // FirstUseTime 属性仅仅是一个示例，可以根据实际需求添加其它相关属性
    [[SensorsAnalyticsSDK sharedInstance] trackInstallation:@"AppInstall" 
                                             withProperties:@{
                                                 @"FirstUseTime" : currentTime}];
}
@end
