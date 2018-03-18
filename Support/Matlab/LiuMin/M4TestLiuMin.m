clear,clc
[s1]=textread('/Users/xuchengkai/Desktop/MachineTestLiuMin/M4/M4TestSeed1.txt','%s','headerlines',1);%此文件适用于4卷轴机台
NumOfItem=length(s1);%Spin总次数
ID=zeros(NumOfItem,1);
ResultType=zeros(NumOfItem,1);%结果的种类：0=普通输；1=中奖；2=差点赢
ResultID=zeros(NumOfItem,1);
Lucky=zeros(NumOfItem,1);
CurrentCredit=zeros(NumOfItem,1);%本次Spin前的Credits
Bet=zeros(NumOfItem,1);
Amount=zeros(NumOfItem,1);
CreditChange=zeros(NumOfItem,1);
RemainCredit=zeros(NumOfItem,1);%本次Spin后的Credits 
LuckyChange=zeros(NumOfItem,1);
RemainLucky=zeros(NumOfItem,1);
for i=1:NumOfItem
    string=strsplit(s1{i},',');
    ID(i)=str2num(string{1});
    ResultType(i)=str2num(string{6});
    ResultID(i)=str2num(string{7});
    Lucky(i)=str2num(string{8});
    CurrentCredit(i)=str2num(string{9});
    Bet(i)=str2num(string{10});
    Amount(i)=str2num(string{11});
    CreditChange(i)=str2num(string{12});
    RemainCredit(i)=str2num(string{13});
    LuckyChange(i)=str2num(string{14});
    RemainLucky(i)=str2num(string{15});
end
TotalOverallHit=0;%总中奖率
TotalNearHit=0;%总差点赢的概率
TotalNormalLoss=0;%总普通输得概率
for i=1:NumOfItem
    if ResultType(i)==1
        TotalOverallHit=TotalOverallHit+1;
    elseif ResultType(i)==2
        TotalNearHit=TotalNearHit+1;
    elseif ResultType(i)==0
        TotalNormalLoss=TotalNormalLoss+1;
    end
end
TotalOverallHit=TotalOverallHit/NumOfItem;
TotalNearHit=TotalNearHit/NumOfItem;
TotalNormalLoss=TotalNormalLoss/NumOfItem; 
TotalBet=0;
TotalWin=0;
Win=zeros(NumOfItem,1);
Expectation=zeros(NumOfItem,1);
for i=1:NumOfItem
    TotalBet=TotalBet+Bet(i);
    Win(i)=CreditChange(i)+Bet(i);
    TotalWin=sum(Win);
    Expectation(i)=TotalWin/TotalBet;%期望值曲线
end
FinalExpectation=Expectation(NumOfItem);
figure(1)
plot(RemainCredit);title('总资产-次数');
hold on
% figure(2)
% plot(Expectation);title('期望值-次数');
% hold on
LuckyLess5000Time=1;
while RemainLucky(LuckyLess5000Time)>=5000
    LuckyLess5000Time=LuckyLess5000Time+1;
end
LuckyRunOutTime=1;
while RemainLucky(LuckyRunOutTime)>0
    LuckyRunOutTime=LuckyRunOutTime+1;
end

fid=fopen('/Users/xuchengkai/Desktop/MachineTestLiuMin/M4/M4TestSeed1_Output.txt','w');
fprintf(fid,'Machine M4\n');
fprintf(fid,'Seed 1\n');
fprintf(fid,'Mode Fix Bet Amount 100\n');
fprintf(fid,'SpinTimes %d\n',NumOfItem);
fprintf(fid,'InitialCredits %d\n',CurrentCredit(1));
fprintf(fid,'FinalCredits %d\n',RemainCredit(NumOfItem));
fprintf(fid,'InitialLucky 50000\n');
fprintf(fid,'LuckyLess5000Time %d\n',LuckyLess5000Time);
fprintf(fid,'LuckyRunOutTime %d\n',LuckyRunOutTime);
fprintf(fid,'FinalLucky %d\n',RemainLucky(NumOfItem));
fprintf(fid,'FinalExpectation %f\n',FinalExpectation);
fprintf(fid,'TotalHitRate %f\n',TotalOverallHit);
fprintf(fid,'TotalNearHit %f\n',TotalNearHit);
fprintf(fid,'TotalNormalLoss %f\n',TotalNormalLoss);
fclose(fid);




