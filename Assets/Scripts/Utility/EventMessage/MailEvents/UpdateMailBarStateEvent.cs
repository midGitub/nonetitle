using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class UpdateMailBarStateEvent : CitrusGameEvent { 
    private string _mailType;
    public string MailType{
        get { return _mailType; }
    }
    public UpdateMailBarStateEvent(string mailType) : base(){
        _mailType = mailType;
    }
}
