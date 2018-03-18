clear,clc
%以下区域为每台机器不同的地方%
[s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11,s12,s13,s14,s15]=textread('/Users/xuchengkai/Desktop/MachineLiuMin/M2/M2LuckyPayout.txt','%d %s %s %d %d %f %f %s %d %s %s %s %s %f %s','headerlines',1);
%以下为变量命名
ID=s1;
Symbols=s2;
PayoutType=s3;
Count=s4;
Ratio=s5;
OverallHit=s6;
FreeSpinOverallHit=s7;
FreeSpinHits=s8;
LongLuckySubtractFactor=s9;
IsFixed=s10;
Reel1Wild=s11;
Reel2Wild=s12;
Reel3Wild=s13;
StandardHit=s14;
IsShortLucky=s15;
%以上区域为每台机器不同的地方%

NumOfItem=length(s1);%中奖组合条目数
TotalHitRate=sum(OverallHit);%普通旋转时的总中奖率
TrueFreeSpinOverallHit=zeros(NumOfItem,1);%FreeSpin时的总中奖率
for i=1:NumOfItem
    TrueFreeSpinOverallHit(i)=FreeSpinOverallHit(i)/sum(FreeSpinOverallHit);
end

%以下区域为每台机器不同的地方%
Reel1WildNum=zeros(NumOfItem,3);%卷轴1上的Wild概率分布表
for i=1:NumOfItem
    string=strsplit(Reel1Wild{i},',');
    for j=1:3
        Reel1WildNum(i,j)=str2num(string{j});
    end
end
Reel2WildNum=zeros(NumOfItem,4);%卷轴2上的Wild概率分布表
for i=1:NumOfItem
    string=strsplit(Reel2Wild{i},',');
    for j=1:4
        Reel2WildNum(i,j)=str2num(string{j});
    end
end
Reel3WildNum=zeros(NumOfItem,3);%卷轴3上的Wild概率分布表
for i=1:NumOfItem
    string=strsplit(Reel3Wild{i},',');
    for j=1:3
        Reel3WildNum(i,j)=str2num(string{j});
    end
end
%以上区域为每台机器不同的地方%

Expectation=zeros(NumOfItem,1);%普通旋转时的期望值的分布表
ExpectationInFreeSpin=zeros(NumOfItem,1);%FreeSpin时的期望值的分布表
PBonusWild=zeros(NumOfItem,1);%BonusWild的概率分布表
for i=1:9
    %Ordered3
    Expectation(i)=OverallHit(i)*Ratio(i);
    ExpectationInFreeSpin(i)=TrueFreeSpinOverallHit(i)*Ratio(i);
    PBonusWild(i)=OverallHit(i);
end
for i=10:16
    %All3
    %按每个卷轴上是否替换为Wild来分类，1表示替换为wild。有8种情况：(000、001、010、011、100、101、110、111)，其中不存在111的情况，把111转化为110、101、011
    %以下区域为每台机器不同的地方%
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);
    R1=R11+R12+R13;
    R2=Reel2WildNum(i,1)+Reel2WildNum(i,2)+Reel2WildNum(i,3)+Reel2WildNum(i,4);
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);R33=Reel3WildNum(i,3);
    R3=R31+R32+R33;
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
    %以下区域为每台机器不同的地方%
    WildMultipleInAll3=0;%因为Wild所带来的倍数，包含了StandardHit的情况
    WildMultipleInAll3=WildMultipleInAll3+P000*1;%添加000带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P001*(R31*5+R32*3+R33*1)/R3;%添加001带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P010*(R2*1)/R2;%添加010带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P011*(R2*1)/R2*(R31*5+R32*3+R33*1)/R3;%添加011带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P100*(R11*5+R12*3+R13*1)/R1;%添加100带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P101*(R11*5+R12*3+R13*1)/R1*(R31*5+R32*3+R33*1)/R3;%添加101带来的基础倍数
    WildMultipleInAll3=WildMultipleInAll3+P110*(R11*5+R12*3+R13*1)/R1*(R2*1)/R2;%添加110带来的基础倍数
    %以上区域为每台机器不同的地方%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAll3;
    ExpectationInFreeSpin(i)=TrueFreeSpinOverallHit(i)*Ratio(i)*WildMultipleInAll3;
    PBonusWild(i)=OverallHit(i)*(P010*R2+P011*R2+P110*R2);
end
for i=17:18
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
    R11=Reel1WildNum(i,1);R12=Reel1WildNum(i,2);R13=Reel1WildNum(i,3);
    R1=R11+R12+R13;
    R2=Reel2WildNum(i,1)+Reel2WildNum(i,2)+Reel2WildNum(i,3)+Reel2WildNum(i,4);
    R31=Reel3WildNum(i,1);R32=Reel3WildNum(i,2);R33=Reel3WildNum(i,3);
    R3=R31+R32+R33; 
    %以上区域为每台机器不同的地方%
    P000=WildPossibility110*(1-R1)*(1-R2)+WildPossibility101*(1-R1)*(1-R3)+WildPossibility011*(1-R2)*(1-R3)+WildPossibility111*(1-R1)*(1-R2)*(1-R3);%组合(R1‘R2’+R1’R3‘+R2‘R3‘+R1'R2'R3'), 表示第一、二、三卷轴都不替换为wild的概率.i.e. StandardHit
    P100=WildPossibility110*(R1*(1-R2)+1/2*R1*R2)+WildPossibility101*(R1*(1-R3)+1/2*R1*R3)+WildPossibility111*(1/3*R1+1/3*(1-R2)*1/2*R1+1/3*(1-R2)*1/2*(1-R3)*R1+1/3*(1-R3)*1/2*R1+1/3*(1-R3)*1/2*(1-R2)*R1);%组合(R1R2'+R1R3'+R1R2'R3'),表示只有第一个卷轴替换为wild的概率.
    P010=WildPossibility110*(R2*(1-R1)+1/2*R1*R2)+WildPossibility011*(R2*(1-R3)+1/2*R2*R3)+WildPossibility111*(1/3*R2+1/3*(1-R1)*1/2*R2+1/3*(1-R1)*1/2*(1-R3)*R2+1/3*(1-R3)*1/2*R2+1/3*(1-R3)*1/2*(1-R1)*R2);%组合(R2R1'+R2R3'+R2R1'R3'),表示只有第二个卷轴替换为wild的概率.
    P001=WildPossibility101*(R3*(1-R1)+1/2*R1*R3)+WildPossibility011*(R3*(1-R2)+1/2*R2*R3)+WildPossibility111*(1/3*R3+1/3*(1-R1)*1/2*R3+1/3*(1-R1)*1/2*(1-R2)*R3+1/3*(1-R2)*1/2*R3+1/3*(1-R2)*1/2*(1-R1)*R3);%组合(R3R1'+R3R2'+R3R1'R2'),表示只有第三个卷轴替换为wild的概率.
    PAny3SymbolsType3=P000+P100+P010+P001;%%验证上述4种情况的概率之和是否为1
    %以下区域为每台机器不同的地方%
    WildMultipleInAny3Symbols4=0;%因为Wild所带来的倍数，包含StandardHit的情况
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P000*1;%添加000带来的基础倍数，即StandardHit的基础倍数
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P100*(R11*5+R12*3+R13*1)/R1;%添加100带来的基础倍数，即第一个卷轴的wild带来的基础倍数
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P010*(R2*1)/R2;%添加010带来的基础倍数，即第二个卷轴的wild带来的基础倍数
    WildMultipleInAny3Symbols4=WildMultipleInAny3Symbols4+P001*(R31*5+R32*3+R33*1)/R3;%添加001带来的基础倍数，即第三个卷轴的wild带来的基础倍数
    %以上区域为每台机器不同的地方%
    Expectation(i)=OverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;  
    ExpectationInFreeSpin(i)=TrueFreeSpinOverallHit(i)*Ratio(i)*WildMultipleInAny3Symbols4;
    PBonusWild(i)=OverallHit(i)*P010*R2;
end

%以下为FreeSpin玩法带来的次数%
FreeSpinHitsNum=zeros(NumOfItem,4);%每个组合FreeSpin概率配置表
FreeSpin=zeros(NumOfItem,100);%每个组合FreeSpin X次的概率
FreeSpinTimes=zeros(NumOfItem,1);%每个组合FreeSpin次数的期望值,不考虑BonusWild出现的概率
FreeSpinTimesExpectation=zeros(NumOfItem,1);%每个组合FreeSpin总次数的期望值，考虑BonusWild出现的概率
for i=1:NumOfItem
    string=strsplit(FreeSpinHits{i},',');
    for j=1:4
        FreeSpinHitsNum(i,j)=str2num(string{j});
    end
    FreeSpin(i,1)=FreeSpinHitsNum(i,1)*(1-FreeSpinHitsNum(i,2));%FreeSpin 1次的概率
    FreeSpin(i,2)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*(1-FreeSpinHitsNum(i,3));%FreeSpin 2次的概率
    FreeSpin(i,3)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)*(1-FreeSpinHitsNum(i,4));%FreeSpin 3次的概率
%     FreeSpin(i,4)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^2*(1-FreeSpinHitsNum(i,3));%FreeSpin 4次的概率
%     FreeSpin(i,5)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^3*(1-FreeSpinHitsNum(i,3));%FreeSpin 5次的概率
%     FreeSpin(i,6)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^4*(1-FreeSpinHitsNum(i,3));%FreeSpin 6次的概率
%     FreeSpin(i,7)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^5*(1-FreeSpinHitsNum(i,3));%FreeSpin 7次的概率
%     FreeSpin(i,8)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)^6*(1-FreeSpinHitsNum(i,3));%FreeSpin 8次的概率
    for m=4:100
        FreeSpin(i,m)=FreeSpinHitsNum(i,1)*FreeSpinHitsNum(i,2)*FreeSpinHitsNum(i,3)*FreeSpinHitsNum(i,4)^(m-3)*(1-FreeSpinHitsNum(i,4));%FreeSpin m次的概率
    end
    for j=1:100
        FreeSpinTimes(i)=FreeSpinTimes(i)+FreeSpin(i,j)*j;
    end
    FreeSpinTimesExpectation(i)=FreeSpinTimes(i)*PBonusWild(i);
end

TotalPBonusWild=sum(PBonusWild);%所有组合BonusWild出现的概率之和，缺了loss的情况下出现wild的概率！
TotalFreeSpinTimesExpectation=sum(FreeSpinTimesExpectation);%所有组合FreeSpin总次数的期望值
TotalExpectation=sum(Expectation);%所有组合普通旋转时的总期望值
TotalExpectationInFreeSpin=sum(ExpectationInFreeSpin);%所有组合FreeSpin时的总期望值
TotalExpectationAfterFreeSpin=sum(Expectation)+sum(ExpectationInFreeSpin)*TotalFreeSpinTimesExpectation;%总期望值

fid=fopen('/Users/xuchengkai/Desktop/MachineLiuMin/M2/M2LuckyPayout_Output.txt','w');
%fprintf(fid,'Normal Mode:\n');
fprintf(fid,'Lucky Mode:\n');
fprintf(fid,'TotalExpectationInNormalSpin %f\n',TotalExpectation);
fprintf(fid,'TotalExpectationInFreeSpin %f\n',TotalExpectationInFreeSpin);
fprintf(fid,'TotalExpectationAfterFreeSpin %f\n',TotalExpectationAfterFreeSpin);
fprintf(fid,'TotalHitRateInNormalSpin %f\n',TotalHitRate);
fprintf(fid,'TotalPBonusWild %f\n',TotalPBonusWild);
fprintf(fid,'TotalFreeSpinTimesExpectation %f\n',TotalFreeSpinTimesExpectation);
fclose(fid);