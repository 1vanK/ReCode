set "PATH=c:\Windows\Microsoft.NET\Framework\v2.0.50727\"
del ReCode.exe
csc /target:winexe /out:ReCode.exe *.cs>Log.txt
