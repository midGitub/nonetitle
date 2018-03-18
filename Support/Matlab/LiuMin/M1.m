clear,clc
%以下区域为每台机器不同的地方%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M1/M1LuckyPayout.txt','%d %s %s %d %f %f %d %s %s %s %s %f %s','headerlines',1);
%以下为变量命名
ID=s1;
Symbols=s2;
PayoutType=s3;
Count=s4;
Ratio=s5;
OverallHit=s6;
LongLuckySubtractFactor=s7;
IsFixed=s8;
Reel1Wild=s9;
Reel2Wild=s10;
Reel3Wild=s11;
StandardHit=s12;
IsShortLucky=s13;
%以上区域为每台机器不同的地方%

NumOfItem=length(s1);%中奖组合条目数
TotalHitRate=sum(OverallHit);%总中奖率

%以下区域为每台机器不同的地方%
Reel1WildNum=zeros(NumOfItem,4);%卷轴1上的Wild概率分布表
for i=1:NumOfItem
    string=strsplit(Reel1Wild{i},',');
    for j=1:4
        Reel1WildNum(i,j)=str2num(string{j});
    end
end
Reel2WildNum=zeros(NumOfItem,3);%卷轴2上的Wild概率分布表
for i=1:NumOfItem
    string=strsplit(Reel2Wild{i},',');
    for j=1:3
        Reel2WildNum(i,j)=str2num(string{j});
    end
end
Reel3WildNum=zeros(NumOfItem,4);%卷轴3上的Wild概率分布表
for i=1:NumOfItem
    string=strsplit(Reel3Wild{i},',');
    for j=1:4
        Reel3WildNum(i,j)=str2num(string{j});
    end
end
%以上区域为每台机器不同的地方%

Expectation=zeros(NumOfItem,1);%期望值的分布表
for i=1:3
    %Ordered3
    Expectation(i)=OverallHit(i)*Ratio(i);
    StandardHit(i)=0;
end
for i=4:10
    %All3
    %按每个卷轴上是否替换为Wild来分类，1表示替换为wild。有8种情况：(000、001、010、011、100、101、110、111)，其中不存在111的情况，把111转化为110、101、011
    %以下区域为每台机器不同的地方%
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%为方便下面的计算，所以简化变量名
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
    %以上区域为每台机器不同的地方%
    P111=R1*R2*R3;%第一、二、三个卷轴有Wild。因为不存在这种情况，所以它的概率被平均分配到110、101、011三种情况中
    P000=(1-R1)*(1-R2)*(1-R3);%第一、二、三个卷轴没有Wild。i.e. StandardHit
    P001=(1-R1)*(1-R2)*R3;%第三个卷轴有Wild
    P010=(1-R1)*R2*(1-R3);%第二个卷轴有Wild
    P011=(1-R1)*R2*R3+1/3*P111;%第二、三个卷轴有Wild,包含了由P111转化过来的部分
    P100=R1*(1-R2)*(1-R3);%第一个卷轴有Wild
    P101=R1*(1-R2)*R3+1/3*P111;%第一、三个卷轴有Wild,包含了由P111转化过来的部分
    P110=R1*R2*(1-R3)+1/3*P111;%第一、二个卷轴有Wild,包含了由P111转化过来的部分
    PAll3=P000+P001+P010+P011+P100+P101+P110;%%验证上述7种情况的概率之和是否为1
    StandardHit(i)=P000;
    %以下区域为每台机器不同的地方%
    WildMultipleInAll3=0;%因为Wild所带来的倍数，包含了StandardHit的情况
    WildMultipleInAll3=WildMultipleInAll3+P000*1;%添加000带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P001*(R3*2)/R3;%添加001带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P010*(R21*3+R22*5+R23*10)/R2;%添加010带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P011*(R21*3+R22*5+R23*10)/R2*(R3*2)/R3;%添加011带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P100*(R1*2)/R1;%添加100带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P101*(R1*2)/R1*(R3*2)/R3;%添加101带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P110*(R1*2)/R1*(R21*3+R22*5+R23*10)/R2;%添加110带来的基础倍数
    %以上区域为每台机器不同的地方%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAll3;
end
for i=11:12
    %Any3,SymbolsType=4。
    %共4^3=64种Symbol排列，设4种symbol分别为A、B、C、D
    %(AAA,BBB,CCC,DDD)=(4个1/64)=>(111)=1/64*4=1/16=>(111->1/3(110),1/3(101),1/3(011))=>(110,101,011)=(1/48,1/48,1/48)
    %(AAB,AAC,AAD,BBA,BBC,BBD,CCA,CCB,CCD,DDA,DDB,DDC)=(12个1/64)=>(110)=1/64*12=3/16=>3/16+1/48=5/24
    %(ABA,ACA,ADA,BAB,BCB,BDB,CAC,CBC,CDC,DAD,DBD,DCD)=(12个1/64)=>(101)=1/64*12=3/16=>3/16+1/48=5/24
    %(ABB,ACC,ADD,BAA,BCC,BDD,CAA,CBB,CDD,DAA,DBB,DCC)=(12个1/64)=>(011)=1/64*12=3/16=>3/16+1/48=5/24
    %(ABC,ABD,ACB,ACD,ADB,ADC,BAC,BAD,BCA,BCD,BDA,BDC,CAB,CAD,CBA,CBD,CDA,CDB,DAB,DAC,DBA,DBC,DCA,DCB)=(24个1/64)=>(111)=1/64*24=3/8
    %按三个卷轴上的是否有相同元素来分类（即按三个卷轴上的元素能否替换为wild来分类），只有在1上才有可能替换为wild（但不一定替换为wild）。有4种情况：（110，101，011,111）=（1/4,3/16,3/16,3/8）.
    WildPossibility110=5/24;%第一、二卷轴的symbol相同的概率（只有第一、二卷轴有替换为wild的可能性）
    WildPossibility101=5/24;%第一、三卷轴的symbol相同的概率（只有第一、三卷轴有替换为wild的可能性）
    WildPossibility011=5/24;%第二、三卷轴的symbol相同的概率（只有第二、三卷轴有替换为wild的可能性）
    WildPossibility111=3/8;%第一、二、三卷轴的symbol都不相同的概率（第一、二、三卷轴都有可能替换为wild）
    %以下区域为每台机器不同的地方%
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%为方便下面的计算，所以简化变量名
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
    %以上区域为每台机器不同的地方%
    P000=WildPossibility110*(1-R1)*(1-R2)+WildPossibility101*(1-R1)*(1-R3)+WildPossibility011*(1-R2)*(1-R3)+WildPossibility111*(1-R1)*(1-R2)*(1-R3);%组合(R1‘R2’+R1’R3‘+R2‘R3‘+R1'R2'R3'), 表示第一、二、三卷轴都不替换为wild的概率.i.e. StandardHit
    P100=WildPossibility110*(R1*(1-R2)+1/2*R1*R2)+WildPossibility101*(R1*(1-R3)+1/2*R1*R3)+WildPossibility111*(1/3*R1+1/3*(1-R2)*1/2*R1+1/3*(1-R2)*1/2*(1-R3)*R1+1/3*(1-R3)*1/2*R1+1/3*(1-R3)*1/2*(1-R2)*R1);%组合(R1R2'+R1R3'+R1R2'R3'),表示只有第一个卷轴替换为wild的概率.
    P010=WildPossibility110*(R2*(1-R1)+1/2*R1*R2)+WildPossibility011*(R2*(1-R3)+1/2*R2*R3)+WildPossibility111*(1/3*R2+1/3*(1-R1)*1/2*R2+1/3*(1-R1)*1/2*(1-R3)*R2+1/3*(1-R3)*1/2*R2+1/3*(1-R3)*1/2*(1-R1)*R2);%组合(R2R1'+R2R3'+R2R1'R3'),表示只有第二个卷轴替换为wild的概率.
    P001=WildPossibility101*(R3*(1-R1)+1/2*R1*R3)+WildPossibility011*(R3*(1-R2)+1/2*R2*R3)+WildPossibility111*(1/3*R3+1/3*(1-R1)*1/2*R3+1/3*(1-R1)*1/2*(1-R2)*R3+1/3*(1-R2)*1/2*R3+1/3*(1-R2)*1/2*(1-R1)*R3);%组合(R3R1'+R3R2'+R3R1'R2'),表示只有第三个卷轴替换为wild的概率.
    PAny3SymbolsType3=P000+P100+P010+P001;%%验证上述4种情况的概率之和是否为1
    StandardHit(i)=P000;
    %以下区域为每台机器不同的地方%
    WildMultipleInAny3Symbols4=0;%因为Wild所带来的倍数，包含StandardHit的情况
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P000*1;%添加000带来的基础倍数，即StandardHit的基础倍数
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P100*(R1*2)/R1;%添加100带来的基础倍数，即第一个卷轴的wild带来的基础倍数
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P010*(R21*3+R22*5+R23*10)/R2;%添加010带来的基础倍数，即第二个卷轴的wild带来的基础倍数
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P001*(R3*2)/R3;%添加001带来的基础倍数，即第三个卷轴的wild带来的基础倍数
    %以上区域为每台机器不同的地方%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;  
end
for i=13 
    %Any2
    StandardHit(i)=0;
    %按哪两个卷轴上有wild元素来分类，有3种情况：(R1R2,R1R3,R2R3)=(1/3,1/3,1/3)
    WildExistence110=1/3;%第一、二卷轴的symbol为wild的概率
    WildExistence101=1/3;%第一、三卷轴的symbol为wild的概率
    WildExistence011=1/3;%第二、三卷轴的symbol为wild的概率
    %以下区域为每台机器不同的地方%
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%为方便下面的计算，所以简化变量名
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
    WildMultipleInAny2=0;%因为Wild所带来的倍数，没有StandardHit的情况
    WildMultipleInAny2=WildMultipleInAny2+WildExistence110*(R1*2)/R1*(R21*3+R22*5+R23*10)/R2;%添加110带来的基础倍数
    WildMultipleInAny2=WildMultipleInAny2+WildExistence101*(R1*2)/R1*(R3*2)/R3;%添加101带来的基础倍数
    WildMultipleInAny2=WildMultipleInAny2+WildExistence011*(R21*3+R22*5+R23*10)/R2*(R3*2)/R3;%添加011带来的基础倍数
    %以上区域为每台机器不同的地方%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny2;
end
for i=14
    %Any1
    StandardHit(i)=0;
    %按哪一个卷轴上有wild元素来分类，有3种情况：(R2,R2,R3)=(1/3,1/3,1/3)
    WildExistence100=1/3;%第一卷轴的symbol为wild的概率
    WildExistence010=1/3;%第二卷轴的symbol为wild的概率
    WildExistence001=1/3;%第三卷轴的symbol为wild的概率
    R1=Reel1WildNum(i,1)+Reel1WildNum(i,2)+Reel1WildNum(i,3)+Reel1WildNum(i,4);%%为方便下面的计算，所以简化变量名
    R21=Reel2WildNum(i,1);R22=Reel2WildNum(i,2);R23=Reel2WildNum(i,3);
    R2=R21+R22+R23;
    R3=Reel3WildNum(i,1)+Reel3WildNum(i,2)+Reel3WildNum(i,3)+Reel3WildNum(i,4);
    WildMultipleInAny1=0;
    WildMultipleInAny1=WildMultipleInAny1+WildExistence100*(R1*2)/R1;
    WildMultipleInAny1=WildMultipleInAny1+WildExistence010*(R21*3+R22*5+R23*10)/R2;
    WildMultipleInAny1=WildMultipleInAny1+WildExistence001*(R3*2)/R3;
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny1;
end
TotalExpectation=sum(Expectation);%总期望值
TotalStandardHit=0;%总标准中奖（无wild）概率
for i=1:NumOfItem
    TotalStandardHit=TotalStandardHit+StandardHit(i)*OverallHit(i);
end

fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M1/M1LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectation %f\n',TotalExpectation);
fprintf(fid,'TotalHitRate %f\n',TotalHitRate);
fprintf(fid,'TotalStandardHit %f\n',TotalStandardHit);
fprintf(fid,'TotalStandardHit/TotalHitRate %f\n',TotalStandardHit/TotalHitRate);
fclose(fid);