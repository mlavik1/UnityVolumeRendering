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

uvr_project_path = os.path.join(os.path.dirname(os.path.realpath(__file__)), os.pardir)
export_project_path = "tmp-package-export"

if os.path.exists(export_project_path):
    shutil.rmtree(export_project_path)
os.mkdir(export_project_path)

assets = ["Assets", "DataFiles", "ACKNOWLEDGEMENTS.txt", "CREDITS.md", "LICENSE", "README.md"]

for asset in assets:
    dest_asset = os.path.join(export_project_path, "Assets", "UnityVolumeRendering", asset)
    copy_filedir(asset, dest_asset)

command_string = "\"{unity_path}\" -projectPath {project_path} -exportPackage Assets UnityVolumeRendering.unitypackage -batchmode -nographics -silent-crashes -quit".format(unity_path=unity_path, project_path=export_project_path)
print(command_string)
os.system(command_string)

shutil.copy(os.path.join(export_project_path, "UnityVolumeRendering.unitypackage"), "UnityVolumeRendering.unitypackage")

shutil.rmtree(export_project_path)
