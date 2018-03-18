/**
 * Created by nichos on 17/7/9.
 */

'use strict';

function getCommandLineValue(key)
{
    var result = null;
    var argvs = process.argv;
    var index = argvs.indexOf(key);
    if(index >= 0 && index < argvs.length - 1) {
        result = argvs[index + 1];
    }
    return result;
}

function capitalizeFirstLetter(str)
{
    return str[0].toUpperCase() + str.slice(1);
}

function lowerFirstLetter(str)
{
    return str[0].toLowerCase() + str.slice(1);
}

exports.getCommandLineValue = getCommandLineValue;
exports.capitalizeFirstLetter = capitalizeFirstLetter;
exports.lowerFirstLetter = lowerFirstLetter;
