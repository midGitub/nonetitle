How to use the script:

1 install node.js

2 run command to install node_modules:
    # npm install

3 run command to upload machine assets:
    # node index.js -platform PLATFORM -build BUILD -machines MACHINES -vpn VPN

    Arguments:
    PLATFORM: iOS or Android
    BUILD: Debug,Release (case insensitive)
    MACHINES: M11,M12,M13
    VPN: 1 or 0, indicate if use internal vpn or not

    Examples:
    # node index.js -platform iOS -build Debug,Release -machines M13,M14,M15 -vpn 1
    # node index.js -platform Android -build Debug,Release -machines M13,M14,M15 -vpn 1
