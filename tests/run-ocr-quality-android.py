"""Runs only the synthetic opt-in OCR QA package on the selected Android emulator.
Does not install/uninstall apps or access photos. The QA APK must already be installed.
"""
import argparse,json,subprocess,time,uuid
from pathlib import Path
parser=argparse.ArgumentParser();parser.add_argument("--serial",default="emulator-5554");parser.add_argument("--adb",default=r"C:\Android\Sdk\platform-tools\adb.exe");args=parser.parse_args()
package="com.companyname.helalharam.ocrqa"
root=Path(__file__).resolve().parents[1];out=root/".verification"/"ocr-quality";out.mkdir(parents=True,exist_ok=True)
def adb(*cmd,check=True):return subprocess.run([args.adb,"-s",args.serial,*cmd],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=check,timeout=30)
if not args.serial.startswith("emulator-"):raise SystemExit("Use the isolated QA app on an emulator.")
def distance(a,b):
    row=list(range(len(b)+1))
    for i,ca in enumerate(a,1):
        previous=row[0];row[0]=i
        for j,cb in enumerate(b,1):
            old=row[j];row[j]=min(row[j]+1,row[j-1]+1,previous+(ca!=cb));previous=old
    return row[-1]
reports=[]
run=str(uuid.uuid4())
for index in range(15):
    adb("shell","am","broadcast","-p",package,"-a","com.companyname.helalharam.OCR_QUALITY","--ei","case",str(index),"--es","run",run)
    deadline=time.monotonic()+45
    while time.monotonic()<deadline:
        result=adb("exec-out","run-as",package,"cat",f"files/ocr-quality-{index}.json",check=False)
        if result.returncode==0:
            try:report=json.loads(result.stdout)
            except (ValueError,UnicodeDecodeError):time.sleep(1);continue
            if report.get("Run")!=run:time.sleep(1);continue
            break
        time.sleep(1)
    else:raise SystemExit(f"No result for case {index}; no successful run claimed")
    if "Error" not in report:
        expected=report["Expected"];actual=report["Recognized"]
        report["ExactRaw"]=actual==expected
        report["RawCharacterErrorRate"]=distance(expected,actual)/len(expected) if expected else (0 if not actual else 1)
    reports.append(report);print(json.dumps(report,ensure_ascii=False),flush=True)
    (out/"results.json").write_text(json.dumps(reports,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("Report:",out/"results.json")
if any("Error" in row for row in reports):raise SystemExit(2)
print("Measurements complete. Imperfect/empty results are measured failures, not a universal OCR guarantee.")
