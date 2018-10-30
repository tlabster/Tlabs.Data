###Some formual samples


latPeriod (Latency period in day after which a doc. is assumed missing)
```
60
```


```
@AfterQDays(prg["DOC"], prg["docInterval"], prg["latPeriod"])
```

DOCmis
```
iif(@IsTaskType(src, "Missing-FD"), src.date, iif(p.misDocCnt == 1, p.DOCmis, null))
```

DOCwarn

