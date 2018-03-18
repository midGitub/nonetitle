/**
 * Created by nichos on 17/7/8.
 */

'use strict';

const aws = require('aws-sdk');
const proxy = require("proxy-agent");
const fs = require('fs');

var credential = require('./credential.json');
const config = require('./config.json');
const uploader = require('./uploader');
const utility = require('./utility');

function initAWSConfig(isVPN)
{
    if(isVPN)
    {
        credential["httpOptions"] = {
            agent: proxy('http://192.168.3.236:8123')
        };
        console.log('use VPN to upload');
    }

    aws.config.update(credential);
}

function upload(platform, builds, machines)
{
    var localPath = config.localResourcePath + platform + '/';

    machines.forEach((m) => {
        var lowerMachienName = utility.lowerFirstLetter(m);

        //version file
        var localVersionFile = localPath + lowerMachienName + '_version';
        var versionData = fs.readFileSync(localVersionFile);
        if(versionData)
        {
            builds.forEach((build) => {
                build = build.toLowerCase();
                var remotePath = config.remoteResourcePaths[build];
                var remoteVersionFile = remotePath + platform + '/' + m + '/' + versionData + '/' + m + '_version';

                console.log('local version file: ' + localVersionFile);
                console.log('remote version file: ' + remoteVersionFile);

                uploader.upload(remoteVersionFile, versionData);
            });
        }
        else
        {
            console.log('Error: read local version file fail:' + localVersionFile);
        }

        //machine asset bundle
        var localFile = localPath + lowerMachienName;
        var fileData = fs.readFileSync(localFile);
        if(fileData)
        {
            builds.forEach((build) => {
                build = build.toLowerCase();
                var remotePath = config.remoteResourcePaths[build];
                var remoteFile = remotePath + platform + '/' + m + '/' + versionData + '/' + m;

                console.log('local machine file: ' + localFile);
                console.log('remote machine file: ' + remoteFile);

                uploader.upload(remoteFile, fileData);
            });
        }
        else
        {
            console.log('Error: read local machine file fail:' + localFile);
        }


    });
}

function run()
{
    //platform
    var platform = utility.getCommandLineValue('-platform');
    if(!platform) {
        console.log('Error: no -platform in command line argument');
        return;
    }

    //build
    var buildString = utility.getCommandLineValue('-build');
    if(!buildString) {
        console.log('Error: no -build in command line argument');
        return;
    }

    var builds = buildString.split(',');
    builds.forEach((build) => {
        var b = build.toLowerCase();
        if(b != "debug" && b != "release")
        {
            console.log("Error: the argument following -build must be Debug/debug/Release/release");
        }
    });

    //machines
    var machinesString = utility.getCommandLineValue('-machines');
    if(!machinesString) {
        console.log('Error: no -machines in command line argument');
        return;
    }

    var machines = machinesString.split(',');

    //vpn
    var isVPN = false;
    var vpnString = utility.getCommandLineValue('-vpn');
    if(vpnString != null && vpnString != "")
        isVPN = (vpnString != "0" && vpnString.length > 0);

    initAWSConfig(isVPN);

    upload(platform, builds, machines);
}

run();
