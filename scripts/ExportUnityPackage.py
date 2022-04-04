import os, shutil, errno

def copy_filedir(src, dst):
    try:
        shutil.copytree(src, dst)
    except OSError as exc: # python >2.5
        if exc.errno in (errno.ENOTDIR, errno.EINVAL):
            shutil.copy(src, dst)
        else: raise

unity_path = "D:/Program Files/UnityEditors/2019.4.35f1/Editor/Unity.exe" # TODO
uvr_project_path = os.path.join(os.path.dirname(os.path.realpath(__file__)), os.pardir)
export_project_path = "tmp-package-export"

if os.path.exists(export_project_path):
    shutil.rmtree(export_project_path)
os.mkdir(export_project_path)

assets = ["Assets", "DataFiles", "ACKNOWLEDGEMENTS.txt", "CREDITS.md", "LICENSE", "README.md"]

for asset in assets:
    dest_asset = os.path.join(export_project_path, "Assets", "UnityVolumeRendering", asset)
    copy_filedir(asset, dest_asset)

command_string = "\"{unity_path}\" -batchmode -nographics -projectPath -silent-crashes {project_path} -exportPackage {assets} UnityVolumeRendering.unitypackage -quit".format(unity_path=unity_path, project_path=export_project_path, assets="Assets")
print(command_string)
os.system(command_string)

shutil.rmtree(export_project_path)
