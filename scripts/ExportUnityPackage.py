import os, shutil, errno, sys

def copy_filedir(src, dst):
    try:
        shutil.copytree(src, dst)
    except OSError as exc: # python >2.5
        if exc.errno in (errno.ENOTDIR, errno.EINVAL):
            shutil.copy(src, dst)
        else: raise

if len(sys.argv) > 1:
    unity_path = str(sys.argv[1])
else:
    unity_path = raw_input("Enter full filepath of Unity executable:")
    
assetstore_package =  "-assetstore" in sys.argv
nodisplay =  "-nodisplay" in sys.argv
package_name = 'EasyVolumeRenderer.unitypackage' if assetstore_package else 'UnityVolumeRendering.unitypackage'
plugin_folder_name = 'EasyVolumeRendering' if assetstore_package else 'UnityVolumeRendering'

uvr_project_path = os.path.join(os.path.dirname(os.path.realpath(__file__)), os.pardir)
export_project_path = "tmp-package-export"

if os.path.exists(export_project_path):
    shutil.rmtree(export_project_path)
os.mkdir(export_project_path)

if assetstore_package:
    os.system("python scripts/DocsToPDF.py")

if assetstore_package:
    with open('Third Party Notices.md', 'r') as original:
        third_party_contents = original.read()
    with open('Third Party Notices.md', 'w') as modified:
        modified.write("This asset is governed by the Asset Store EULA; however, the following components are governed by the licenses indicated below:\n" + third_party_contents)

if assetstore_package:
    assets = [
        ("Runtime", "Runtime"),
        ("Editor", "Editor"),
        ("ThirdParty", "ThirdParty"),
        (os.path.join("SampleProject~", "SampleData"), "SampleData"),
        ("Third Party Notices.md", "Third Party Notices.md"),
        (os.path.join("Documentation~", "MANUAL.pdf"), "MANUAL.pdf"),
    ]
else:
    assets = [
        ("Runtime", "Runtime"),
        ("Editor", "Editor"),
        ("ThirdParty", "ThirdParty"),
        (os.path.join("SampleProject~", "SampleData"), "SampleData"),
        ("Third Party Notices.md", "Third Party Notices.md"),
        ("CREDITS.md", "CREDITS.md"),
        ("LICENSE.md", "LICENSE.md"),
        ("README.md", "README.md"),
    ]

for src, dest_name in assets:
    dest_asset = os.path.join(export_project_path, "Assets", plugin_folder_name, dest_name)
    copy_filedir(src, dest_asset)

command_string = "\"{unity_path}\" -projectPath {project_path} -exportPackage Assets {package_name} -batchmode -nographics -silent-crashes -quit".format(unity_path=unity_path, project_path=export_project_path, package_name=package_name)
# Run through cvfb if no display available (building in container, etc.).
if nodisplay:
    command_string = "xvfb-run --auto-servernum --server-args=\'-screen 0 640x480x24\' " + command_string
print(command_string)
os.system(command_string)

shutil.copy(os.path.join(export_project_path, package_name), package_name)

shutil.rmtree(export_project_path)
