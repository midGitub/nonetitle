/**
 * Created by nichos on 17/7/8.
 */

'use strict';

const aws = require('aws-sdk');
const config = require('./config.json');
var s3 = null;

function lazyInitS3()
{
    if(!s3) s3 = new aws.S3();
}

function upload(key, obj)
{
    lazyInitS3();
    var params = {
        Bucket : config.bucket,
        Key : key,
        Body : obj,
        ACL : "public-read"
    };
    s3.putObject(params, function(err, data) {
        if(err)
            console.log("Upload error: " + err);
        else
            console.log('Succeed to upload object: ' + key);
    });
}

exports.upload = upload;
