apt install coreutils
echo "$UNITY_LICENSE" | base64 -d > unity_license.ulf
ls
ls /opt/unity/Editor/
echo "Activating unity license..."
/opt/unity/Editor/Unity -batchmode -nographics -quit -manualLicenseFile unity_license.ulf || true
