clear,clc
[s1]=textread('/Users/xuchengkai/Desktop/MachineTestLiuMin/M1/M1TestSeed5.txt','%s','headerlines',1);%���ļ�������3�����̨
NumOfItem=length(s1);%Spin�ܴ���
ID=zeros(NumOfItem,1);
ResultType=zeros(NumOfItem,1);%��������ࣺ0=��ͨ�䣻1=�н���2=���Ӯ
ResultID=zeros(NumOfItem,1);
Lucky=zeros(NumOfItem,1);
CurrentCredit=zeros(NumOfItem,1);%����Spinǰ��Credits
Bet=zeros(NumOfItem,1);
Amount=zeros(NumOfItem,1);
CreditChange=zeros(NumOfItem,1);
RemainCredit=zeros(NumOfItem,1);%����Spin���Credits 
LuckyChange=zeros(NumOfItem,1);
RemainLucky=zeros(NumOfItem,1);
for i=1:NumOfItem
    string=strsplit(s1{i},',');
    ID(i)=str2num(string{1});
    ResultType(i)=str2num(string{5});
    ResultID(i)=str2num(string{6});
    Lucky(i)=str2num(string{7});
    CurrentCredit(i)=str2num(string{8});
    Bet(i)=str2num(string{9});
    Amount(i)=str2num(string{10});
    CreditChange(i)=str2num(string{11});
    RemainCredit(i)=str2num(string{12});
    LuckyChange(i)=str2num(string{13});
    RemainLucky(i)=str2num(string{14});
end
LuckyLess5000Time=1;%LuckyֵС��5000ʱ����ת�Ĵ���
while RemainLucky(LuckyLess5000Time)>=5000
    LuckyLess5000Time=LuckyLess5000Time+1;
end
LuckyRunOutTime=1;%Luckyֵ����0ʱ����ת�Ĵ���
while RemainLucky(LuckyRunOutTime)>0
    LuckyRunOutTime=LuckyRunOutTime+1;
end
CreditLess500Time=LuckyRunOutTime;%CreditС��500ʱ����ת�Ĵ���
while RemainCredit(CreditLess500Time)>500
    CreditLess500Time=CreditLess500Time+1;
end
TotalOverallHit=0;%���н���
TotalNearHit=0;%�ܲ��Ӯ�ĸ���
TotalNormalLoss=0;%����ͨ��ø���
TotalOverallHitInLucky=0;%Luckyģ�������н���
TotalNearHitInLucky=0;%Luckyģ�����ܲ��Ӯ�ĸ���
TotalNormalLossInLucky=0;%Luckyģ��������ͨ��ø���
TotalOverallHitInMix=0;%���ģ�������н���
TotalNearHitInMix=0;%���ģ�����ܲ��Ӯ�ĸ���
TotalNormalLossInMix=0;%���ģ��������ͨ��ø���
TotalOverallHitInNormal=0;%��ͨģ�������н���
TotalNearHitInNormal=0;%��ͨģ�����ܲ��Ӯ�ĸ���
TotalNormalLossInNormal=0;%��ͨģ��������ͨ��ø���
for i=1:NumOfItem
    if ResultType(i)==1%���������
        TotalOverallHit=TotalOverallHit+1;
    elseif ResultType(i)==2
        TotalNearHit=TotalNearHit+1;
    elseif ResultType(i)==0
        TotalNormalLoss=TotalNormalLoss+1;
    end
    if i<=LuckyLess5000Time%����ģ����
        if ResultType(i)==1
            TotalOverallHitInLucky=TotalOverallHitInLucky+1;
        elseif ResultType(i)==2
            TotalNearHitInLucky=TotalNearHitInLucky+1;
        elseif ResultType(i)==0
            TotalNormalLossInLucky=TotalNormalLossInLucky+1;
        end
    end
    if i>LuckyLess5000Time && i<LuckyRunOutTime%���ģ����
        if ResultType(i)==1
            TotalOverallHitInMix=TotalOverallHitInMix+1;
        elseif ResultType(i)==2
            TotalNearHitInMix=TotalNearHitInMix+1;
        elseif ResultType(i)==0
            TotalNormalLossInMix=TotalNormalLossInMix+1;
        end
    end
    if i>=LuckyRunOutTime%��ͨģ����
        if ResultType(i)==1
            TotalOverallHitInNormal=TotalOverallHitInNormal+1;
        elseif ResultType(i)==2
            TotalNearHitInNormal=TotalNearHitInNormal+1;
        elseif ResultType(i)==0
            TotalNormalLossInNormal=TotalNormalLossInNormal+1;
        end
    end     
end
TotalOverallHit=TotalOverallHit/NumOfItem;%���������
TotalNearHit=TotalNearHit/NumOfItem;
TotalNormalLoss=TotalNormalLoss/NumOfItem; 
TotalOverallHitInLucky=TotalOverallHitInLucky/LuckyLess5000Time;%Luckyģ����
TotalNearHitInLucky=TotalNearHitInLucky/LuckyLess5000Time;
TotalNormalLossInLucky=TotalNormalLossInLucky/LuckyLess5000Time; 
TotalOverallHitInMix=TotalOverallHitInMix/(LuckyRunOutTime-LuckyLess5000Time-1);%���ģ����
TotalNearHitInMix=TotalNearHitInMix/(LuckyRunOutTime-LuckyLess5000Time-1);
TotalNormalLossInMix=TotalNormalLossInMix/(LuckyRunOutTime-LuckyLess5000Time-1); 
TotalOverallHitInNormal=TotalOverallHitInNormal/(NumOfItem-LuckyRunOutTime+1);%��ͨģ����
TotalNearHitInNormal=TotalNearHitInNormal/(NumOfItem-LuckyRunOutTime+1);
TotalNormalLossInNormal=TotalNormalLossInNormal/(NumOfItem-LuckyRunOutTime+1); 

TotalBet=0;%���������
TotalWin=0;
Win=zeros(NumOfItem,1);
Expectation=zeros(NumOfItem,1);
TotalBetInLucky=0;%Luckyģ����
TotalWinInLucky=0;
WinInLucky=zeros(NumOfItem,1);
ExpectationInLucky=zeros(NumOfItem,1);
TotalBetInMix=0;%���ģ����
TotalWinInMix=0;
WinInMix=zeros(NumOfItem,1);
ExpectationInMix=zeros(NumOfItem,1);
TotalBetInNormal=0;%��ͨģ����
TotalWinInNormal=0;
WinInNormal=zeros(NumOfItem,1);
ExpectationInNormal=zeros(NumOfItem,1);
for i=1:NumOfItem
    TotalBet=TotalBet+Bet(i);%���������
    Win(i)=CreditChange(i)+Bet(i);
    TotalWin=sum(Win);
    Expectation(i)=TotalWin/TotalBet;%����ֵ����
    if i<=LuckyLess5000Time%����ģ����
        TotalBetInLucky=TotalBetInLucky+Bet(i);
        WinInLucky(i)=CreditChange(i)+Bet(i);
        TotalWinInLucky=sum(WinInLucky);
        ExpectationInLucky(i)=TotalWinInLucky/TotalBetInLucky;
    end
    if i>LuckyLess5000Time && i<LuckyRunOutTime%���ģ����
        TotalBetInMix=TotalBetInMix+Bet(i);
        WinInMix(i)=CreditChange(i)+Bet(i);
        TotalWinInMix=sum(WinInMix);
        ExpectationInMix(i)=TotalWinInMix/TotalBetInMix;
    end
    if i>=LuckyRunOutTime%��ͨģ����
        TotalBetInNormal=TotalBetInNormal+Bet(i);
        WinInNormal(i)=CreditChange(i)+Bet(i);
        TotalWinInNormal=sum(WinInNormal);
        ExpectationInNormal(i)=TotalWinInNormal/TotalBetInNormal;
    end
end
FinalExpectation=Expectation(NumOfItem);
FinalExpectationInLucky=ExpectationInLucky(LuckyLess5000Time);
FinalExpectationInMix=ExpectationInMix(LuckyRunOutTime-1);
FinalExpectationInNormal=ExpectationInNormal(NumOfItem);
figure(1)
plot(RemainCredit);title('���ʲ�-����');
hold on
fid=fopen('/Users/xuchengkai/Desktop/MachineTestLiuMin/M1/M1TestSeed5_Output.txt','w');
fprintf(fid,'Auto-Test Configure\n');
fprintf(fid,'Machine M1\n');
fprintf(fid,'Seed 5\n');
fprintf(fid,'Mode Fix Bet Percentage 5\n');
fprintf(fid,'SpinTimes %d\n',NumOfItem);
fprintf(fid,'InitialCredits %d\n',CurrentCredit(1));
fprintf(fid,'InitialLucky 50000\n\n');
fprintf(fid,'Result\n');
fprintf(fid,'FinalCredits %d\n',RemainCredit(NumOfItem));
fprintf(fid,'FinalLucky %d\n',RemainLucky(NumOfItem));
fprintf(fid,'LuckyLess5000Times %d (The Times the reel has spun When Lucky<5000��\n',LuckyLess5000Time);
fprintf(fid,'LuckyRunOutTimes %d ��The Times the reel has spun When Lucky=0��\n',LuckyRunOutTime);
fprintf(fid,'CreditLess500Times %d ��The Times the reel has spun When Credit<500��\n\n',CreditLess500Time);
fprintf(fid,'Overall Result(SpinTime:1-%d)\n',NumOfItem);
fprintf(fid,'FinalExpectation %f\n',FinalExpectation);
fprintf(fid,'TotalHitRate %f\n',TotalOverallHit);
fprintf(fid,'TotalNearHit %f\n',TotalNearHit);
fprintf(fid,'TotalNormalLoss %f\n\n',TotalNormalLoss);
fprintf(fid,'Lucky Mode Result(SpinTime:1-%d)\n',LuckyLess5000Time);
fprintf(fid,'FinalExpectationInLucky %f\n',FinalExpectationInLucky);
fprintf(fid,'TotalHitRateInLucky %f\n',TotalOverallHitInLucky);
fprintf(fid,'TotalNearHitInLucky %f\n',TotalNearHitInLucky);
fprintf(fid,'TotalNormalLossInLucky %f\n\n',TotalNormalLossInLucky);
fprintf(fid,'Mix Mode Result(SpinTime:%d-%d)\n',LuckyLess5000Time+1,LuckyRunOutTime-1);
fprintf(fid,'FinalExpectationInMix %f\n',FinalExpectationInMix);
fprintf(fid,'TotalHitRateInMix %f\n',TotalOverallHitInMix);
fprintf(fid,'TotalNearHitInMix %f\n',TotalNearHitInMix);
fprintf(fid,'TotalNormalLossInMix %f\n\n',TotalNormalLossInMix);
fprintf(fid,'Normal Mode Result(SpinTime:%d-%d)\n',LuckyRunOutTime,NumOfItem);
fprintf(fid,'FinalExpectationInNormal %f\n',FinalExpectationInNormal);
fprintf(fid,'TotalHitRateInNormal %f\n',TotalOverallHitInNormal);
fprintf(fid,'TotalNearHitInNormal %f\n',TotalNearHitInNormal);
fprintf(fid,'TotalNormalLossInNormal %f\n\n',TotalNormalLossInNormal);
fprintf(fid,'Period of Win\n');
WinPeriod=1;
for i=1:NumOfItem
    if ResultType(i)==1
        fprintf(fid,'%d ',WinPeriod);
        WinPeriod=1;
    else
        WinPeriod=WinPeriod+1;
    end
end
fclose(fid);


