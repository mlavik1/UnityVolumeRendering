import os, shutil, errno, sys, subprocess

thirdparty_excludes = {"SimpleITK", "SimpleITK.meta", "SimpleITK.zip", "SimpleITK.zip.meta"}

def ignore_files(dir, names):
    if os.path.normpath(dir) == os.path.normpath("ThirdParty"):
        return [name for name in names if name in thirdparty_excludes]
    return []

def copy_filedir(src, dst):
    try:
        shutil.copytree(src, dst, ignore=ignore_files)
    except OSError as exc: # python >2.5
        if exc.errno in (errno.ENOTDIR, errno.EINVAL):
            shutil.copy(src, dst)
        else: raise

if len(sys.argv) > 1:
    unity_path = str(sys.argv[1])
else:
    unity_path = input("Enter full filepath of Unity executable:")

nodisplay =  "-nodisplay" in sys.argv
package_name = 'MingRay.unitypackage'
plugin_folder_name = 'MingRay'

uvr_project_path = os.path.join(os.path.dirname(os.path.realpath(__file__)), os.pardir)
os.chdir(uvr_project_path)
export_project_path = "tmp-package-export"

if os.path.exists(export_project_path):
    shutil.rmtree(export_project_path)
os.mkdir(export_project_path)

assets = [
    ("Runtime", "Runtime"),
    ("Editor", "Editor"),
    ("ThirdParty", "ThirdParty"),
    (os.path.join("Samples~", "SampleData"), "SampleData"),
    (os.path.join("Samples~", "BasicDemo"), "BasicDemo"),
    ("DONOTREMOVE-PathSearchFile.txt", "DONOTREMOVE-PathSearchFile.txt"),
    ("Third Party Notices.md", "Third Party Notices.md"),
    ("CHANGELOG.md", "CHANGELOG.md"),
    ("CREDITS.md", "CREDITS.md"),
    ("LICENSE.md", "LICENSE.md"),
    ("README.md", "README.md"),
]

for src, dest_name in assets:
    dest_asset = os.path.join(export_project_path, "Assets", plugin_folder_name, dest_name)
    copy_filedir(src, dest_asset)

command = [unity_path, "-projectPath", export_project_path, "-exportPackage", "Assets", package_name, "-batchmode", "-nographics", "-silent-crashes", "-quit"]
# Run through cvfb if no display available (building in container, etc.).
if nodisplay:
    command = ["xvfb-run", "--auto-servernum", "--server-args=-screen 0 640x480x24"] + command
print(subprocess.list2cmdline(command))
result = subprocess.run(command)
if result.returncode != 0:
    sys.exit("Unity export failed with exit code {code}".format(code=result.returncode))

shutil.copy(os.path.join(export_project_path, package_name), package_name)

shutil.rmtree(export_project_path)
