/**
 * Created by nichos on 17/7/9.
 */

'use strict';

const fs = require('fs');
const uploader = require('./uploader');

function testUpload()
{
    var data = fs.readFileSync('test.txt');
    uploader.upload('slots-debug/machineassets/t1.txt', data);
}

