import os
import glob
import shutil
import sys
import zipfile
import urllib.request

def zip_files_in_folder(folder_path, zip_file_path):
    with zipfile.ZipFile(zip_file_path, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as zipf:
        for foldername, subfolders, filenames in os.walk(folder_path):
            for filename in filenames:
                file_path = os.path.join(foldername, filename)
                zipf.write(file_path, arcname=os.path.basename(file_path))
    print(f"📦 压缩包已生成: {zip_file_path}")

def md_to_pdf(file_name):
    os.system(f"pandoc --pdf-engine=xelatex  -V mainfont=LXGWWenKaiMono-Regular.ttf -V geometry:margin=0.5in --template eisvogel.tex  {file_name} -o {file_name.replace('.md', '.pdf')}")

if __name__ == '__main__':
    print(f"🚀 开始执行打包脚本...(By Cai 😋)")
    build_type = sys.argv[1]
    print(f"🗑️ 开始删除json文件...")
    for file in glob.glob(os.path.join(f"out/{build_type}/", "*.json")):
        os.remove(file)
        print(f"✅ 已删除文件: {file}")
    print("✅ json文件删除成功！")

    print("📝 开始移动README.md...")
    cwd = os.getcwd()
    for dir_name in os.listdir(cwd):
        dir_path = os.path.join(cwd, dir_name)
        if os.path.isdir(dir_path):
            for file_name in os.listdir(dir_path):
                try:
                    if file_name.endswith('.csproj'):
                        source_path = os.path.join(dir_path, 'README.md')
                        destination_path = os.path.join(cwd, 'out', f'{build_type}', file_name.replace('.csproj', '.md'))
                        shutil.copyfile(source_path, destination_path)
                        print(f"🔍 找到README.md({destination_path})")
                except:
                    print(f"⚠️ README移动失败({file_name})")
    shutil.copyfile('README.md', f"out/{build_type}/README.md")
    print("✅ README.md移动成功！")
    
    print("📦 准备打包插件...")
    zip_files_in_folder("out", "Plugins.zip")
    print("🎉 插件打包成功！")
