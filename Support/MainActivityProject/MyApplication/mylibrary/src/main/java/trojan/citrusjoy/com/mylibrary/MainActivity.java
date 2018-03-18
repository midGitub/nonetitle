package trojan.citrusjoy.com.mylibrary;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;

import com.facebook.FacebookSdk;

import com.facebook.appevents.AppEventsLogger;
import com.facebook.unity.UnityMessage;
import com.facebook.applinks.AppLinkData;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import android.util.Log;

/**
 * Created by daiwenhao on 17/6/14.
 */
public class MainActivity extends UnityPlayerActivity{
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (!FacebookSdk.isInitialized())
            FacebookSdk.sdkInitialize(getApplicationContext());

        AppLinkData.fetchDeferredAppLinkData(this.getApplicationContext(),
                new AppLinkData.CompletionHandler() {
                    @Override
                    public void onDeferredAppLinkDataFetched(AppLinkData appLinkData) {
                        // Process app link data
                        if (appLinkData != null) {
                            String uri = appLinkData.getTargetUri().toString();
                            UnityPlayer.UnitySendMessage("AnalysisManager", "DeepLink", uri);
                            Log.d("Deep", "Receive Link Data");
                        }
                    }
                }
        );
    }

//    @Override
//    protected void onResume() {
//        super.onResume();
//        SendOpenEvent();
//    }
//
//    private void SendOpenEvent(){
//        Intent startingIntent = this.getIntent();
//        Bundle pushData = startingIntent.getBundleExtra("push");
//        if (pushData != null){
//            // TODO: Unity接口调用，计数器加一。然后在游戏里判断计数器是否增加，如果增加就进行打点。
//            UnityPlayer.UnitySendMessage("Main Camera", "DoTest", "");
//        }
//    }
}
