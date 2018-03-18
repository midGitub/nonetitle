using System.Collections;
using System.Collections.Generic;

public class NotifyIDFactory {
	private static readonly int DEFAULT_VALUE = 987654321;
	private static readonly int BASE_ID_MULTIPLY = 1000000;// LocalNotification表格里的id的乘算基准值
	private static readonly int BASE_FESTIVAL_ID_MULTIPLY = 1000;// 节日类推送id乘算基准值
	private static readonly int INVALID_VALUE = -1;

	// local推送id算法
	// id * base_id_multiply + index

	// 节日推送id算法
	// id * base_festival_id_multiply + index

	public static bool IsFestivalNotification(int id){
		return id >= BASE_FESTIVAL_ID_MULTIPLY && id < BASE_ID_MULTIPLY;
	}

	public static bool IsLocalNotification(int id){
		return id >= BASE_ID_MULTIPLY && id != DEFAULT_VALUE;
	}

	public static int CreateFestivalID(int id, int index = 0){
		return BASE_FESTIVAL_ID_MULTIPLY * id + index;
	}

	public static int CreateLocalID(int id, int index = 0){
		return BASE_ID_MULTIPLY * id + index;
	}

	private static int ParseFestivalID(int id){
		int result = INVALID_VALUE;
		if (id >= BASE_ID_MULTIPLY){
			CoreDebugUtility.Assert(false, "id is more than local notification");
		}else {
			int mod = id % BASE_FESTIVAL_ID_MULTIPLY;
			int value = ( id - mod ) / BASE_FESTIVAL_ID_MULTIPLY;
			result = value;
		}
		return result;
	}

	private static int ParseLocalID(int id){
		int result = INVALID_VALUE;
		if (id < BASE_ID_MULTIPLY){
			CoreDebugUtility.Assert(false, "id is small than local notification");
		}else{
			int mod = id % BASE_ID_MULTIPLY;
			int value = ( id - mod ) / BASE_ID_MULTIPLY;
			result = value;
		}
		return result;
	}

	public static int ParseNotifyID(int id){
		int result = INVALID_VALUE;
		if (id == DEFAULT_VALUE) {
		}else if (id >= BASE_ID_MULTIPLY){
			result = ParseLocalID(id);
		}else if (id >= BASE_FESTIVAL_ID_MULTIPLY){
			result = ParseFestivalID(id);
		}else if (id > 0){
			result = id;
		}
		CoreDebugUtility.Assert(result != INVALID_VALUE, "ParseNotifyID = " + id);
		return result;
	}
}
