
# ==== .ARGB_SYNC_effect_NO (line 838)
send_hex = [
 0,82,66,64,0,149,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"同步灯效写入：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_dengxiao_speed (line 850)
send_hex = [
 0,82,66,64,0,150,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"流速写入：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_fangxiang (line 862)
send_hex = [
 0,82,66,64,0,151,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"方向写入：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_liangdu (line 874)
send_hex = [
 0,82,66,64,0,153,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"方向写入：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_yanse (line 886)
one_list_colors = [color for row in two_list_colors for color in iter(row)]
send_hex = [0,82,66,64,0,154] + one_list_colors
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"灯效颜色写入：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_shiyin_modle (line 899)
send_hex = [
 0,82,66,64,0,155]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"进入电脑拾音：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_shiyin_vaule (line 911)
send_hex = [
 0,82,66,64,0,156,vaule] + sync_rgb_list
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
print(f"电脑拾音：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_shiyin_vaule_argb (line 923)
send_hex = [
 0,82,66,64,0,4,dengxiao_sn] + sync_rgb_list + [sound_vaule]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex = send_hex + [send_hex_sum]
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_BOOT (line 936)
send_hex = [
 0,82,66,64,0,255]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
print(f"重启进入boot：{send_hex}, 长度：{len(send_hex)}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_Uploader (line 949)
import subprocess
time.sleep(0.5)
hex = '"C:\\Kittle_Work\\ziyanxiangmu\\K18-Feelin Kit\\Lower_computer_software\\CH32X035F7P6\\k18_usb_same_screen\\PIOC\\1_Wire\\obj\\1_Wire.hex"'
cmd_command = f"2.exe -c .\\ch32.INI -f {hex} -o -download"
print("正在更新设备固件，请稍等")
process = subprocess.Popen(cmd_command, shell=True, stdout=(subprocess.PIPE), stderr=(subprocess.PIPE))
process.wait()
print("固件更新完成")
# ==== .ARGB_IAP (line 960)
send_hex = [
 0,82,66,6,0,254]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
print(f"使重启进入IAP：{send_hex}, 长度：{len(send_hex)}")
try:
    print
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"写入失败: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_Uploader_IAP (line 988)
import hid
ser = None
while True:
    hid_list = hid.enumerate()[::-1]
    for device_info in hid_list:
        if device_info["vendor_id"] == 6790:
            if device_info["product_id"] == 1:
                if device_info["interface_number"] == 0:
                    print("找到 待升级的 ARGB设备")
                    port.ARGB_HEX_SN = 0
                    ser = hid.device()
                    ser.open_path(device_info["path"])
                    break
                if device_info["vendor_id"] == 6790:
                    if device_info["product_id"] == 2:
                        if device_info["interface_number"] == 0:
                            print("找到 待升级的 ARGB设备2")
                            port.ARGB_HEX_SN = 1
                            ser = hid.device()
                            ser.open_path(device_info["path"])
                            break
                        time.sleep(0.1)

    if ser is not None:
        break

if ser is not None:
    hid_upload(ser)
# ==== .ARGB_SYNC_speed (line 1131)
send_hex = [
 0,82,66,64,0,157,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"风扇调速异常: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_pwm_speed (line 1142)
send_hex = [
 0,82,66,64,0,251,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"风扇速度：{send_hex}")
except Exception as e:
    try:
        print(f"风扇调速异常: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_MCU_C_changer (line 1154)
send_hex = [
 0,82,66,64,0,253,num]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
print(f"灯光控制权：{send_hex}")
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"切换风扇灯光控制报错: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_SYNC_same_screen (line 1166)
send_hex = [
 0,82,66,64,0,158] + sync_rgb_list
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"同屏数据发送异常: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_ten_effect (line 1178)
send_hex = [
 0,82,66,64,0,3,order,num] + fan_order_list
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    print(f"0x03 :{send_hex}")
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"单设备灯效指令异常: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_ten_effect_set (line 1189)
send_hex = [
 0,82,66,64,0,3,order,num] + fan_order_list
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    print("单设备设置灯效")
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"单设备灯效指令异常: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_dan_config (line 1203)
send_hex = [
 0,82,66,64,0,2,id,function,parameter]
send_hex[3] = len(send_hex)
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    print(f"单设备信息设置：{send_hex}")
    ser.write(send_hex)
except Exception as e:
    try:
        print(f"RGB风扇 设置单设备信息更改异常: {e}")
    finally:
        e = None
        del e
# ==== .ARGB_dan_duqu (line 1215)
# decompile failed (ParserError(COLLECTION_START, '0_0', False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       ARGB_dan_duqu
# # Filename:          agreement.py
# # Argument count:    2
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  6
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1215
# # Constants:
# #    0: None
# #    1: 0
# #    2: 82
# #    3: 66
# #    4: 7
# #    5: 1
# #    6: 256
# #    7: 64
# #    8: 500
# #    9: ('timeout_ms',)
# #   10: 6
# #   11: 10
# #   12: 'RGB风扇 获取单设备信息异常: '
# # Names:
# #    0: sum
# #    1: append
# #    2: write
# #    3: print
# #    4: read
# #    5: Exception
# # Varnames:
# #	ser, id, send_hex, send_hex_sum, data, e
# # Positional arguments:
# #	ser, id
# # Local variables:
# #    2: send_hex
# #    3: send_hex_sum
# #    4: data
# #    5: e
# 
# # Method Name:       ARGB_dan_duqu
# # Filename:          agreement.py
# # Argument count:    2
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  6
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1215
# # Constants:
# #    0: None
# #    1: 0
# #    2: 82
# #    3: 66
# #    4: 7
# #    5: 1
# #    6: 256
# #    7: 64
# #    8: 500
# #    9: ('timeout_ms',)
# #   10: 6
# #   11: 10
# #   12: 'RGB风扇 获取单设备信息异常: '
# # Names:
# #    0: sum
# #    1: append
# #    2: write
# #    3: print
# #    4: read
# #    5: Exception
# # Varnames:
# #	ser, id, send_hex, send_hex_sum, data, e
# # Positional arguments:
# #	ser, id
# # Local variables:
# #    2: send_hex
# #    3: send_hex_sum
# #    4: data
# #    5: e
# 1216:           0 LOAD_CONST           (0)
#                2 LOAD_CONST           (82)
#                4 LOAD_CONST           (66)
#                6 LOAD_CONST           (7)
#                8 LOAD_CONST           (0)
#               10 LOAD_CONST           (1)
#               12 LOAD_FAST            (id)
#               14 BUILD_LIST           7
#               16 STORE_FAST           (send_hex)
# 
# 1217:          18 LOAD_GLOBAL          (sum)
#               20 LOAD_FAST            (send_hex)
#               22 CALL_FUNCTION        (1 positional argument)
#               24 LOAD_CONST           (256)
#               26 BINARY_MODULO
#               28 STORE_FAST           (send_hex_sum)
# 
# 1218:          30 LOAD_FAST            (send_hex)
#               32 LOAD_METHOD          (append)
#               34 LOAD_FAST            (send_hex_sum)
#               36 CALL_METHOD          (1 positional argument)
#               38 POP_TOP
# 
# 1219:          40 SETUP_FINALLY        (to 114)
# 
# 1220:          42 LOAD_FAST            (ser)
#               44 LOAD_METHOD          (write)
#               46 LOAD_FAST            (send_hex)
#               48 CALL_METHOD          (1 positional argument)
#               50 POP_TOP
# 
# 1221:          52 LOAD_GLOBAL          (print)
#               54 LOAD_FAST            (send_hex)
#               56 CALL_FUNCTION        (1 positional argument)
#               58 POP_TOP
# 
# 1222:          60 LOAD_FAST            (ser)
#               62 LOAD_ATTR            (read)
#               64 LOAD_CONST           (64)
#               66 LOAD_CONST           (500)
#               68 LOAD_CONST           (('timeout_ms',))
#               70 CALL_FUNCTION_KW     (2 total positional and keyword args)
#               72 STORE_FAST           (data)
# 
# 1223:          74 LOAD_GLOBAL          (print)
#               76 LOAD_FAST            (data)
#               78 CALL_FUNCTION        (1 positional argument)
#               80 POP_TOP
# 
# 1224:          82 LOAD_FAST            (data)
#               84 BUILD_LIST           0
#               86 COMPARE_OP           (!=)
#               88 POP_JUMP_IF_FALSE    (to 104)
# 
# 1225:          90 LOAD_FAST            (data)
#               92 LOAD_CONST           (6)
#               94 LOAD_CONST           (10)
#               96 BUILD_SLICE          2
#               98 BINARY_SUBSCR
#              100 POP_BLOCK
#              102 RETURN_VALUE
# 
# 1227:     >>  104 POP_BLOCK
#              106 LOAD_CONST           (None)
#              108 RETURN_VALUE
#              110 POP_BLOCK
#              112 JUMP_FORWARD         (to 168)
# 
# 1228:     >>  114 DUP_TOP
#              116 LOAD_GLOBAL          (Exception)
#              118 COMPARE_OP           (exception-match)
#              120 POP_JUMP_IF_FALSE    (to 166)
#              122 POP_TOP
#              124 STORE_FAST           (e)
#              126 POP_TOP
#              128 SETUP_FINALLY        (to 154)
# 
# 1229:         130 LOAD_GLOBAL          (print)
#              132 LOAD_CONST           ("RGB风扇 获取单设备信息异常: ")
#              134 LOAD_FAST            (e)
#              136 FORMAT_VALUE         0
#              138 BUILD_STRING         2
#              140 CALL_FUNCTION        (1 positional argument)
#              142 POP_TOP
# 
# 1230:         144 POP_BLOCK
#              146 POP_EXCEPT
#              148 CALL_FINALLY         (to 154)
#              150 LOAD_CONST           (None)
#              152 RETURN_VALUE
#          >>  154 LOAD_CONST           (None)
#              156 STORE_FAST           (e)
#              158 DELETE_FAST          (e)
#              160 END_FINALLY
#              162 POP_EXCEPT
#              164 JUMP_FORWARD         (to 168)
#          >>  166 END_FINALLY
#          >>  168 LOAD_CONST           (None)
#              170 RETURN_VALUE
# 
# 
# ==== .ARGB_hex_check_ver (line 1301)
# decompile failed (ParserError(LOAD_GLOBAL, 0, False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       ARGB_hex_check_ver
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  5
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1301
# # Constants:
# #    0: None
# #    1: 0
# #    2: 3
# #    3: 1
# #    4: '下载URL：'
# #    5: 'Detected proxy:'
# #    6: 'No proxy detected or proxy is disabled.'
# #    7: 8
# #    8: ('timeout',)
# #    9: ('timeout', 'proxies')
# #   10: 'hex_ver'
# #   11: '检测固件最新版本为：'
# #   12: True
# #   13: 'HEX固件版本需要升级'
# #   14: False
# #   15: '检查HEX升级文件JSON下载错误'
# # Names:
# #    0: port
# #    1: GONG_NEI
# #    2: ARGB_HEX_SN
# #    3: hex_uploader_address
# #    4: PID_COMPAY_LIST
# #    5: PID_COMPAY
# #    6: zhengshi_hex_uploader
# #    7: zhengshi_hex_uploader2
# #    8: test_hex_uploader
# #    9: test_hex_uploader2
# #   10: print
# #   11: get_windows_proxy
# #   12: requests
# #   13: get
# #   14: json
# #   15: fan_new_hex_config
# #   16: ARGB_HEX_VER_NEW
# #   17: compare_versions
# #   18: ARGB_HEX_VER
# #   19: fan_flag_updata
# #   20: Exception
# # Varnames:
# #	url, proxies, prox, response, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: e
# 
# # Method Name:       ARGB_hex_check_ver
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  5
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1301
# # Constants:
# #    0: None
# #    1: 0
# #    2: 3
# #    3: 1
# #    4: '下载URL：'
# #    5: 'Detected proxy:'
# #    6: 'No proxy detected or proxy is disabled.'
# #    7: 8
# #    8: ('timeout',)
# #    9: ('timeout', 'proxies')
# #   10: 'hex_ver'
# #   11: '检测固件最新版本为：'
# #   12: True
# #   13: 'HEX固件版本需要升级'
# #   14: False
# #   15: '检查HEX升级文件JSON下载错误'
# # Names:
# #    0: port
# #    1: GONG_NEI
# #    2: ARGB_HEX_SN
# #    3: hex_uploader_address
# #    4: PID_COMPAY_LIST
# #    5: PID_COMPAY
# #    6: zhengshi_hex_uploader
# #    7: zhengshi_hex_uploader2
# #    8: test_hex_uploader
# #    9: test_hex_uploader2
# #   10: print
# #   11: get_windows_proxy
# #   12: requests
# #   13: get
# #   14: json
# #   15: fan_new_hex_config
# #   16: ARGB_HEX_VER_NEW
# #   17: compare_versions
# #   18: ARGB_HEX_VER
# #   19: fan_flag_updata
# #   20: Exception
# # Varnames:
# #	url, proxies, prox, response, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: e
# 1309:           0 LOAD_GLOBAL          (port)
#                2 LOAD_ATTR            (GONG_NEI)
#                4 LOAD_CONST           (0)
#                6 COMPARE_OP           (==)
#                8 POP_JUMP_IF_FALSE    (to 88)
# 
# 1310:          10 LOAD_GLOBAL          (port)
#               12 LOAD_ATTR            (ARGB_HEX_SN)
#               14 LOAD_CONST           (0)
#               16 COMPARE_OP           (==)
#               18 POP_JUMP_IF_FALSE    (to 48)
# 
# 1311:          20 LOAD_GLOBAL          (port)
#               22 LOAD_ATTR            (hex_uploader_address)
#               24 LOAD_GLOBAL          (port)
#               26 LOAD_ATTR            (PID_COMPAY_LIST)
#               28 LOAD_GLOBAL          (port)
#               30 LOAD_ATTR            (PID_COMPAY)
#               32 BINARY_SUBSCR
#               34 LOAD_CONST           (3)
#               36 BINARY_SUBSCR
#               38 BINARY_ADD
#               40 LOAD_GLOBAL          (port)
#               42 LOAD_ATTR            (zhengshi_hex_uploader)
#               44 BINARY_ADD
#               46 STORE_FAST           (url)
# 
# 1312:     >>   48 LOAD_GLOBAL          (port)
#               50 LOAD_ATTR            (ARGB_HEX_SN)
#               52 LOAD_CONST           (1)
#               54 COMPARE_OP           (==)
#               56 POP_JUMP_IF_FALSE    (to 164)
# 
# 1313:          58 LOAD_GLOBAL          (port)
#               60 LOAD_ATTR            (hex_uploader_address)
#               62 LOAD_GLOBAL          (port)
#               64 LOAD_ATTR            (PID_COMPAY_LIST)
#               66 LOAD_GLOBAL          (port)
#               68 LOAD_ATTR            (PID_COMPAY)
#               70 BINARY_SUBSCR
#               72 LOAD_CONST           (3)
#               74 BINARY_SUBSCR
#               76 BINARY_ADD
#               78 LOAD_GLOBAL          (port)
#               80 LOAD_ATTR            (zhengshi_hex_uploader2)
#               82 BINARY_ADD
#               84 STORE_FAST           (url)
#               86 JUMP_FORWARD         (to 164)
# 
# 1315:     >>   88 LOAD_GLOBAL          (port)
#               90 LOAD_ATTR            (ARGB_HEX_SN)
#               92 LOAD_CONST           (0)
#               94 COMPARE_OP           (==)
#               96 POP_JUMP_IF_FALSE    (to 126)
# 
# 1316:          98 LOAD_GLOBAL          (port)
#              100 LOAD_ATTR            (hex_uploader_address)
#              102 LOAD_GLOBAL          (port)
#              104 LOAD_ATTR            (PID_COMPAY_LIST)
#              106 LOAD_GLOBAL          (port)
#              108 LOAD_ATTR            (PID_COMPAY)
#              110 BINARY_SUBSCR
#              112 LOAD_CONST           (3)
#              114 BINARY_SUBSCR
#              116 BINARY_ADD
#              118 LOAD_GLOBAL          (port)
#              120 LOAD_ATTR            (test_hex_uploader)
#              122 BINARY_ADD
#              124 STORE_FAST           (url)
# 
# 1317:     >>  126 LOAD_GLOBAL          (port)
#              128 LOAD_ATTR            (ARGB_HEX_SN)
#              130 LOAD_CONST           (1)
#              132 COMPARE_OP           (==)
#              134 POP_JUMP_IF_FALSE    (to 164)
# 
# 1318:         136 LOAD_GLOBAL          (port)
#              138 LOAD_ATTR            (hex_uploader_address)
#              140 LOAD_GLOBAL          (port)
#              142 LOAD_ATTR            (PID_COMPAY_LIST)
#              144 LOAD_GLOBAL          (port)
#              146 LOAD_ATTR            (PID_COMPAY)
#              148 BINARY_SUBSCR
#              150 LOAD_CONST           (3)
#              152 BINARY_SUBSCR
#              154 BINARY_ADD
#              156 LOAD_GLOBAL          (port)
#              158 LOAD_ATTR            (test_hex_uploader2)
#              160 BINARY_ADD
#              162 STORE_FAST           (url)
# 
# 1319:     >>  164 LOAD_GLOBAL          (print)
#              166 LOAD_CONST           ("下载URL：")
#              168 LOAD_FAST            (url)
#              170 FORMAT_VALUE         0
#              172 BUILD_STRING         2
#              174 CALL_FUNCTION        (1 positional argument)
#              176 POP_TOP
# 
# 1322:         178 SETUP_FINALLY        (to 342)
# 
# 1323:         180 LOAD_GLOBAL          (get_windows_proxy)
#              182 CALL_FUNCTION        (0 positional arguments)
#              184 STORE_FAST           (proxies)
# 
# 1324:         186 LOAD_FAST            (proxies)
#              188 POP_JUMP_IF_FALSE    (to 202)
# 
# 1325:         190 LOAD_GLOBAL          (print)
#              192 LOAD_CONST           ("Detected proxy:")
#              194 LOAD_FAST            (proxies)
#              196 CALL_FUNCTION        (2 positional arguments)
#              198 POP_TOP
#              200 JUMP_FORWARD         (to 210)
# 
# 1327:     >>  202 LOAD_GLOBAL          (print)
#              204 LOAD_CONST           ("No proxy detected or proxy is disabled.")
#              206 CALL_FUNCTION        (1 positional argument)
#              208 POP_TOP
# 
# 1328:     >>  210 LOAD_GLOBAL          (get_windows_proxy)
#              212 CALL_FUNCTION        (0 positional arguments)
#              214 STORE_FAST           (prox)
# 
# 1329:         216 LOAD_FAST            (prox)
#              218 LOAD_CONST           (None)
#              220 COMPARE_OP           (is)
#              222 POP_JUMP_IF_FALSE    (to 240)
# 
# 1330:         224 LOAD_GLOBAL          (requests)
#              226 LOAD_ATTR            (get)
#              228 LOAD_FAST            (url)
#              230 LOAD_CONST           (8)
#              232 LOAD_CONST           (('timeout',))
#              234 CALL_FUNCTION_KW     (2 total positional and keyword args)
#              236 STORE_FAST           (response)
#              238 JUMP_FORWARD         (to 256)
# 
# 1332:     >>  240 LOAD_GLOBAL          (requests)
#              242 LOAD_ATTR            (get)
#              244 LOAD_FAST            (url)
#              246 LOAD_CONST           (8)
#              248 LOAD_FAST            (prox)
#              250 LOAD_CONST           (('timeout', 'proxies'))
#              252 CALL_FUNCTION_KW     (3 total positional and keyword args)
#              254 STORE_FAST           (response)
# 
# 1333:     >>  256 LOAD_FAST            (response)
#              258 LOAD_METHOD          (json)
#              260 CALL_METHOD          (0 positional arguments)
#              262 STORE_GLOBAL         (fan_new_hex_config)
# 
# 1334:         264 LOAD_GLOBAL          (fan_new_hex_config)
#              266 LOAD_CONST           ("hex_ver")
#              268 BINARY_SUBSCR
#              270 LOAD_GLOBAL          (port)
#              272 STORE_ATTR           (ARGB_HEX_VER_NEW)
# 
# 1335:         274 LOAD_GLOBAL          (print)
#              276 LOAD_CONST           ("检测固件最新版本为：")
#              278 LOAD_GLOBAL          (port)
#              280 LOAD_ATTR            (ARGB_HEX_VER_NEW)
#              282 FORMAT_VALUE         0
#              284 BUILD_STRING         2
#              286 CALL_FUNCTION        (1 positional argument)
#              288 POP_TOP
# 
# 1336:         290 LOAD_GLOBAL          (compare_versions)
#              292 LOAD_GLOBAL          (port)
#              294 LOAD_ATTR            (ARGB_HEX_VER_NEW)
#              296 LOAD_GLOBAL          (port)
#              298 LOAD_ATTR            (ARGB_HEX_VER)
#              300 CALL_FUNCTION        (2 positional arguments)
#              302 EXTENDED_ARG         (256)
#              304 POP_JUMP_IF_FALSE    (to 326)
# 
# 1337:         306 LOAD_CONST           (True)
#              308 LOAD_GLOBAL          (port)
#              310 STORE_ATTR           (fan_flag_updata)
# 
# 1338:         312 LOAD_GLOBAL          (print)
#              314 LOAD_CONST           ("HEX固件版本需要升级")
#              316 CALL_FUNCTION        (1 positional argument)
#              318 POP_TOP
# 
# 1339:         320 POP_BLOCK
#              322 LOAD_CONST           (True)
#              324 RETURN_VALUE
# 
# 1341:     >>  326 LOAD_CONST           (False)
#              328 LOAD_GLOBAL          (port)
#              330 STORE_ATTR           (fan_flag_updata)
# 
# 1342:         332 POP_BLOCK
#              334 LOAD_CONST           (False)
#              336 RETURN_VALUE
#              338 POP_BLOCK
#              340 JUMP_FORWARD         (to 398)
# 
# 1343:     >>  342 DUP_TOP
#              344 LOAD_GLOBAL          (Exception)
#              346 COMPARE_OP           (exception-match)
#              348 EXTENDED_ARG         (256)
#              350 POP_JUMP_IF_FALSE    (to 396)
#              352 POP_TOP
#              354 STORE_FAST           (e)
#              356 POP_TOP
#              358 SETUP_FINALLY        (to 384)
# 
# 1344:         360 LOAD_GLOBAL          (print)
#              362 LOAD_CONST           ("检查HEX升级文件JSON下载错误")
#              364 LOAD_FAST            (e)
#              366 FORMAT_VALUE         0
#              368 BUILD_STRING         2
#              370 CALL_FUNCTION        (1 positional argument)
#              372 POP_TOP
# 
# 1345:         374 POP_BLOCK
#              376 POP_EXCEPT
#              378 CALL_FINALLY         (to 384)
#              380 LOAD_CONST           (None)
#              382 RETURN_VALUE
#          >>  384 LOAD_CONST           (None)
#              386 STORE_FAST           (e)
#              388 DELETE_FAST          (e)
#              390 END_FINALLY
#              392 POP_EXCEPT
#              394 JUMP_FORWARD         (to 398)
#          >>  396 END_FINALLY
#          >>  398 LOAD_CONST           (None)
#              400 RETURN_VALUE
# 
# 
# ==== .ARGB_new_hex_down (line 1349)
# decompile failed (ParserError(LOAD_GLOBAL, 0, False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       ARGB_new_hex_down
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  8
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1349
# # Constants:
# #    0: None
# #    1: 'down_url'
# #    2: '准备下载的固件版本：'
# #    3: 'hex_ver'
# #    4: 'Detected proxy:'
# #    5: 'No proxy detected or proxy is disabled.'
# #    6: 8
# #    7: ('timeout',)
# #    8: ('timeout', 'proxies')
# #    9: 'hex_md5'
# #   10: '下载固件校验成功'
# #   11: True
# #   12: '下载固件校验失败'
# #   13: False
# #   14: '下载固件失败'
# # Names:
# #    0: fan_new_hex_config
# #    1: log
# #    2: logger
# #    3: info
# #    4: get_windows_proxy
# #    5: print
# #    6: requests
# #    7: get
# #    8: raise_for_status
# #    9: content
# #   10: bytes
# #   11: hashlib
# #   12: md5
# #   13: hexdigest
# #   14: app_bin_data
# #   15: Exception
# #   16: error
# # Varnames:
# #	url, proxies, prox, response, hex_data, bytes_data, md5_hash, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: hex_data
# #    5: bytes_data
# #    6: md5_hash
# #    7: e
# 
# # Method Name:       ARGB_new_hex_down
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  8
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1349
# # Constants:
# #    0: None
# #    1: 'down_url'
# #    2: '准备下载的固件版本：'
# #    3: 'hex_ver'
# #    4: 'Detected proxy:'
# #    5: 'No proxy detected or proxy is disabled.'
# #    6: 8
# #    7: ('timeout',)
# #    8: ('timeout', 'proxies')
# #    9: 'hex_md5'
# #   10: '下载固件校验成功'
# #   11: True
# #   12: '下载固件校验失败'
# #   13: False
# #   14: '下载固件失败'
# # Names:
# #    0: fan_new_hex_config
# #    1: log
# #    2: logger
# #    3: info
# #    4: get_windows_proxy
# #    5: print
# #    6: requests
# #    7: get
# #    8: raise_for_status
# #    9: content
# #   10: bytes
# #   11: hashlib
# #   12: md5
# #   13: hexdigest
# #   14: app_bin_data
# #   15: Exception
# #   16: error
# # Varnames:
# #	url, proxies, prox, response, hex_data, bytes_data, md5_hash, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: hex_data
# #    5: bytes_data
# #    6: md5_hash
# #    7: e
# 1352:           0 LOAD_GLOBAL          (fan_new_hex_config)
#                2 LOAD_CONST           ("down_url")
#                4 BINARY_SUBSCR
#                6 STORE_FAST           (url)
# 
# 1353:           8 LOAD_GLOBAL          (log)
#               10 LOAD_ATTR            (logger)
#               12 LOAD_METHOD          (info)
#               14 LOAD_CONST           ("准备下载的固件版本：")
#               16 LOAD_GLOBAL          (fan_new_hex_config)
#               18 LOAD_CONST           ("hex_ver")
#               20 BINARY_SUBSCR
#               22 FORMAT_VALUE         0
#               24 BUILD_STRING         2
#               26 CALL_METHOD          (1 positional argument)
#               28 POP_TOP
# 
# 1356:          30 SETUP_FINALLY        (to 196)
# 
# 1357:          32 LOAD_GLOBAL          (get_windows_proxy)
#               34 CALL_FUNCTION        (0 positional arguments)
#               36 STORE_FAST           (proxies)
# 
# 1358:          38 LOAD_FAST            (proxies)
#               40 POP_JUMP_IF_FALSE    (to 54)
# 
# 1359:          42 LOAD_GLOBAL          (print)
#               44 LOAD_CONST           ("Detected proxy:")
#               46 LOAD_FAST            (proxies)
#               48 CALL_FUNCTION        (2 positional arguments)
#               50 POP_TOP
#               52 JUMP_FORWARD         (to 62)
# 
# 1361:     >>   54 LOAD_GLOBAL          (print)
#               56 LOAD_CONST           ("No proxy detected or proxy is disabled.")
#               58 CALL_FUNCTION        (1 positional argument)
#               60 POP_TOP
# 
# 1362:     >>   62 LOAD_GLOBAL          (get_windows_proxy)
#               64 CALL_FUNCTION        (0 positional arguments)
#               66 STORE_FAST           (prox)
# 
# 1363:          68 LOAD_FAST            (prox)
#               70 LOAD_CONST           (None)
#               72 COMPARE_OP           (is)
#               74 POP_JUMP_IF_FALSE    (to 92)
# 
# 1364:          76 LOAD_GLOBAL          (requests)
#               78 LOAD_ATTR            (get)
#               80 LOAD_FAST            (url)
#               82 LOAD_CONST           (8)
#               84 LOAD_CONST           (('timeout',))
#               86 CALL_FUNCTION_KW     (2 total positional and keyword args)
#               88 STORE_FAST           (response)
#               90 JUMP_FORWARD         (to 108)
# 
# 1366:     >>   92 LOAD_GLOBAL          (requests)
#               94 LOAD_ATTR            (get)
#               96 LOAD_FAST            (url)
#               98 LOAD_CONST           (8)
#              100 LOAD_FAST            (prox)
#              102 LOAD_CONST           (('timeout', 'proxies'))
#              104 CALL_FUNCTION_KW     (3 total positional and keyword args)
#              106 STORE_FAST           (response)
# 
# 1367:     >>  108 LOAD_FAST            (response)
#              110 LOAD_METHOD          (raise_for_status)
#              112 CALL_METHOD          (0 positional arguments)
#              114 POP_TOP
# 
# 1368:         116 LOAD_FAST            (response)
#              118 LOAD_ATTR            (content)
#              120 STORE_FAST           (hex_data)
# 
# 1370:         122 LOAD_GLOBAL          (bytes)
#              124 LOAD_FAST            (hex_data)
#              126 CALL_FUNCTION        (1 positional argument)
#              128 STORE_FAST           (bytes_data)
# 
# 1371:         130 LOAD_GLOBAL          (hashlib)
#              132 LOAD_METHOD          (md5)
#              134 LOAD_FAST            (bytes_data)
#              136 CALL_METHOD          (1 positional argument)
#              138 LOAD_METHOD          (hexdigest)
#              140 CALL_METHOD          (0 positional arguments)
#              142 STORE_FAST           (md5_hash)
# 
# 1372:         144 LOAD_FAST            (md5_hash)
#              146 LOAD_GLOBAL          (fan_new_hex_config)
#              148 LOAD_CONST           ("hex_md5")
#              150 BINARY_SUBSCR
#              152 COMPARE_OP           (==)
#              154 POP_JUMP_IF_FALSE    (to 178)
# 
# 1373:         156 LOAD_GLOBAL          (log)
#              158 LOAD_ATTR            (logger)
#              160 LOAD_METHOD          (info)
#              162 LOAD_CONST           ("下载固件校验成功")
#              164 CALL_METHOD          (1 positional argument)
#              166 POP_TOP
# 
# 1374:         168 LOAD_FAST            (bytes_data)
#              170 STORE_GLOBAL         (app_bin_data)
# 
# 1375:         172 POP_BLOCK
#              174 LOAD_CONST           (True)
#              176 RETURN_VALUE
# 
# 1377:     >>  178 LOAD_GLOBAL          (print)
#              180 LOAD_CONST           ("下载固件校验失败")
#              182 CALL_FUNCTION        (1 positional argument)
#              184 POP_TOP
# 
# 1378:         186 POP_BLOCK
#              188 LOAD_CONST           (False)
#              190 RETURN_VALUE
#              192 POP_BLOCK
#              194 JUMP_FORWARD         (to 256)
# 
# 1379:     >>  196 DUP_TOP
#              198 LOAD_GLOBAL          (Exception)
#              200 COMPARE_OP           (exception-match)
#              202 EXTENDED_ARG         (0)
#              204 POP_JUMP_IF_FALSE    (to 254)
#              206 POP_TOP
#              208 STORE_FAST           (e)
#              210 POP_TOP
#              212 SETUP_FINALLY        (to 242)
# 
# 1380:         214 LOAD_GLOBAL          (log)
#              216 LOAD_ATTR            (logger)
#              218 LOAD_METHOD          (error)
#              220 LOAD_CONST           ("下载固件失败")
#              222 LOAD_FAST            (e)
#              224 FORMAT_VALUE         0
#              226 BUILD_STRING         2
#              228 CALL_METHOD          (1 positional argument)
#              230 POP_TOP
# 
# 1381:         232 POP_BLOCK
#              234 POP_EXCEPT
#              236 CALL_FINALLY         (to 242)
#              238 LOAD_CONST           (False)
#              240 RETURN_VALUE
#          >>  242 LOAD_CONST           (None)
#              244 STORE_FAST           (e)
#              246 DELETE_FAST          (e)
#              248 END_FINALLY
#              250 POP_EXCEPT
#              252 JUMP_FORWARD         (to 256)
#          >>  254 END_FINALLY
#          >>  256 LOAD_CONST           (None)
#              258 RETURN_VALUE
# 
# 
# ==== .ARGB_exe_check (line 1395)
# decompile failed (ParserError(SETUP_FINALLY, '0_2', False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       ARGB_exe_check
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  5
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1395
# # Constants:
# #    0: None
# #    1: 3
# #    2: 'exejson地址：'
# #    3: 'Detected proxy:'
# #    4: 'No proxy detected or proxy is disabled.'
# #    5: 8
# #    6: ('timeout',)
# #    7: ('timeout', 'proxies')
# #    8: 4
# #    9: 2
# #   10: 1
# #   11: 'exe_ver_1'
# #   12: 'exe_ver_2'
# #   13: 'exe_ver_3'
# #   14: 'exe_ver'
# #   15: 5
# #   16: 'exe_ver_5'
# #   17: 10
# #   18: 'EXE版本号：'
# #   19: 'exe_down_config :'
# #   20: True
# #   21: False
# #   22: '检查EXE升级文件JSON下载错误:'
# # Names:
# #    0: port
# #    1: hex_uploader_address
# #    2: PID_COMPAY_LIST
# #    3: PID_COMPAY
# #    4: exe_uploader_address
# #    5: print
# #    6: get_windows_proxy
# #    7: requests
# #    8: get
# #    9: json
# #   10: exe_down_config
# #   11: exe_ver
# #   12: compare_versions
# #   13: Software_version
# #   14: EXE_updata_flag
# #   15: Exception
# #   16: log
# #   17: logger
# #   18: error
# # Varnames:
# #	url, proxies, prox, response, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: e
# 
# # Method Name:       ARGB_exe_check
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  5
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1395
# # Constants:
# #    0: None
# #    1: 3
# #    2: 'exejson地址：'
# #    3: 'Detected proxy:'
# #    4: 'No proxy detected or proxy is disabled.'
# #    5: 8
# #    6: ('timeout',)
# #    7: ('timeout', 'proxies')
# #    8: 4
# #    9: 2
# #   10: 1
# #   11: 'exe_ver_1'
# #   12: 'exe_ver_2'
# #   13: 'exe_ver_3'
# #   14: 'exe_ver'
# #   15: 5
# #   16: 'exe_ver_5'
# #   17: 10
# #   18: 'EXE版本号：'
# #   19: 'exe_down_config :'
# #   20: True
# #   21: False
# #   22: '检查EXE升级文件JSON下载错误:'
# # Names:
# #    0: port
# #    1: hex_uploader_address
# #    2: PID_COMPAY_LIST
# #    3: PID_COMPAY
# #    4: exe_uploader_address
# #    5: print
# #    6: get_windows_proxy
# #    7: requests
# #    8: get
# #    9: json
# #   10: exe_down_config
# #   11: exe_ver
# #   12: compare_versions
# #   13: Software_version
# #   14: EXE_updata_flag
# #   15: Exception
# #   16: log
# #   17: logger
# #   18: error
# # Varnames:
# #	url, proxies, prox, response, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: e
# 1398:           0 EXTENDED_ARG         (256)
#                2 SETUP_FINALLY        (to 420)
# 
# 1400:           4 LOAD_GLOBAL          (port)
#                6 LOAD_ATTR            (hex_uploader_address)
#                8 LOAD_GLOBAL          (port)
#               10 LOAD_ATTR            (PID_COMPAY_LIST)
#               12 LOAD_GLOBAL          (port)
#               14 LOAD_ATTR            (PID_COMPAY)
#               16 BINARY_SUBSCR
#               18 LOAD_CONST           (3)
#               20 BINARY_SUBSCR
#               22 BINARY_ADD
#               24 LOAD_GLOBAL          (port)
#               26 LOAD_ATTR            (exe_uploader_address)
#               28 BINARY_ADD
#               30 STORE_FAST           (url)
# 
# 1401:          32 LOAD_GLOBAL          (print)
#               34 LOAD_CONST           ("exejson地址：")
#               36 LOAD_FAST            (url)
#               38 FORMAT_VALUE         0
#               40 BUILD_STRING         2
#               42 CALL_FUNCTION        (1 positional argument)
#               44 POP_TOP
# 
# 1403:          46 LOAD_GLOBAL          (get_windows_proxy)
#               48 CALL_FUNCTION        (0 positional arguments)
#               50 STORE_FAST           (proxies)
# 
# 1404:          52 LOAD_FAST            (proxies)
#               54 POP_JUMP_IF_FALSE    (to 68)
# 
# 1405:          56 LOAD_GLOBAL          (print)
#               58 LOAD_CONST           ("Detected proxy:")
#               60 LOAD_FAST            (proxies)
#               62 CALL_FUNCTION        (2 positional arguments)
#               64 POP_TOP
#               66 JUMP_FORWARD         (to 76)
# 
# 1407:     >>   68 LOAD_GLOBAL          (print)
#               70 LOAD_CONST           ("No proxy detected or proxy is disabled.")
#               72 CALL_FUNCTION        (1 positional argument)
#               74 POP_TOP
# 
# 1408:     >>   76 LOAD_GLOBAL          (get_windows_proxy)
#               78 CALL_FUNCTION        (0 positional arguments)
#               80 STORE_FAST           (prox)
# 
# 1409:          82 LOAD_FAST            (prox)
#               84 LOAD_CONST           (None)
#               86 COMPARE_OP           (is)
#               88 POP_JUMP_IF_FALSE    (to 106)
# 
# 1410:          90 LOAD_GLOBAL          (requests)
#               92 LOAD_ATTR            (get)
#               94 LOAD_FAST            (url)
#               96 LOAD_CONST           (8)
#               98 LOAD_CONST           (('timeout',))
#              100 CALL_FUNCTION_KW     (2 total positional and keyword args)
#              102 STORE_FAST           (response)
#              104 JUMP_FORWARD         (to 122)
# 
# 1412:     >>  106 LOAD_GLOBAL          (requests)
#              108 LOAD_ATTR            (get)
#              110 LOAD_FAST            (url)
#              112 LOAD_CONST           (8)
#              114 LOAD_FAST            (prox)
#              116 LOAD_CONST           (('timeout', 'proxies'))
#              118 CALL_FUNCTION_KW     (3 total positional and keyword args)
#              120 STORE_FAST           (response)
# 
# 1413:     >>  122 LOAD_FAST            (response)
#              124 LOAD_METHOD          (json)
#              126 CALL_METHOD          (0 positional arguments)
#              128 STORE_GLOBAL         (exe_down_config)
# 
# 1415:         130 LOAD_GLOBAL          (port)
#              132 LOAD_ATTR            (PID_COMPAY)
#              134 LOAD_CONST           (4)
#              136 COMPARE_OP           (==)
#              138 EXTENDED_ARG         (256)
#              140 POP_JUMP_IF_FALSE    (to 276)
# 
# 1416:         142 LOAD_GLOBAL          (port)
#              144 LOAD_ATTR            (PID_COMPAY_LIST)
#              146 LOAD_CONST           (4)
#              148 BINARY_SUBSCR
#              150 LOAD_CONST           (2)
#              152 BINARY_SUBSCR
#              154 LOAD_CONST           (1)
#              156 COMPARE_OP           (==)
#              158 POP_JUMP_IF_FALSE    (to 168)
# 
# 1417:         160 LOAD_GLOBAL          (exe_down_config)
#              162 LOAD_CONST           ("exe_ver_1")
#              164 BINARY_SUBSCR
#              166 STORE_GLOBAL         (exe_ver)
# 
# 1418:     >>  168 LOAD_GLOBAL          (port)
#              170 LOAD_ATTR            (PID_COMPAY_LIST)
#              172 LOAD_CONST           (4)
#              174 BINARY_SUBSCR
#              176 LOAD_CONST           (2)
#              178 BINARY_SUBSCR
#              180 LOAD_CONST           (2)
#              182 COMPARE_OP           (==)
#              184 POP_JUMP_IF_FALSE    (to 194)
# 
# 1419:         186 LOAD_GLOBAL          (exe_down_config)
#              188 LOAD_CONST           ("exe_ver_2")
#              190 BINARY_SUBSCR
#              192 STORE_GLOBAL         (exe_ver)
# 
# 1420:     >>  194 LOAD_GLOBAL          (port)
#              196 LOAD_ATTR            (PID_COMPAY_LIST)
#              198 LOAD_CONST           (4)
#              200 BINARY_SUBSCR
#              202 LOAD_CONST           (2)
#              204 BINARY_SUBSCR
#              206 LOAD_CONST           (3)
#              208 COMPARE_OP           (==)
#              210 POP_JUMP_IF_FALSE    (to 220)
# 
# 1421:         212 LOAD_GLOBAL          (exe_down_config)
#              214 LOAD_CONST           ("exe_ver_3")
#              216 BINARY_SUBSCR
#              218 STORE_GLOBAL         (exe_ver)
# 
# 1422:     >>  220 LOAD_GLOBAL          (port)
#              222 LOAD_ATTR            (PID_COMPAY_LIST)
#              224 LOAD_CONST           (4)
#              226 BINARY_SUBSCR
#              228 LOAD_CONST           (2)
#              230 BINARY_SUBSCR
#              232 LOAD_CONST           (4)
#              234 COMPARE_OP           (==)
#              236 POP_JUMP_IF_FALSE    (to 246)
# 
# 1423:         238 LOAD_GLOBAL          (exe_down_config)
#              240 LOAD_CONST           ("exe_ver")
#              242 BINARY_SUBSCR
#              244 STORE_GLOBAL         (exe_ver)
# 
# 1424:     >>  246 LOAD_GLOBAL          (port)
#              248 LOAD_ATTR            (PID_COMPAY_LIST)
#              250 LOAD_CONST           (4)
#              252 BINARY_SUBSCR
#              254 LOAD_CONST           (2)
#              256 BINARY_SUBSCR
#              258 LOAD_CONST           (5)
#              260 COMPARE_OP           (==)
#              262 EXTENDED_ARG         (256)
#              264 POP_JUMP_IF_FALSE    (to 354)
# 
# 1425:         266 LOAD_GLOBAL          (exe_down_config)
#              268 LOAD_CONST           ("exe_ver_5")
#              270 BINARY_SUBSCR
#              272 STORE_GLOBAL         (exe_ver)
#              274 JUMP_FORWARD         (to 354)
# 
# 1426:     >>  276 LOAD_GLOBAL          (port)
#              278 LOAD_ATTR            (PID_COMPAY)
#              280 LOAD_CONST           (10)
#              282 COMPARE_OP           (==)
#              284 EXTENDED_ARG         (256)
#              286 POP_JUMP_IF_FALSE    (to 346)
# 
# 1427:         288 LOAD_GLOBAL          (port)
#              290 LOAD_ATTR            (PID_COMPAY_LIST)
#              292 LOAD_CONST           (10)
#              294 BINARY_SUBSCR
#              296 LOAD_CONST           (2)
#              298 BINARY_SUBSCR
#              300 LOAD_CONST           (10)
#              302 COMPARE_OP           (==)
#              304 EXTENDED_ARG         (256)
#              306 POP_JUMP_IF_FALSE    (to 316)
# 
# 1428:         308 LOAD_GLOBAL          (exe_down_config)
#              310 LOAD_CONST           ("exe_ver")
#              312 BINARY_SUBSCR
#              314 STORE_GLOBAL         (exe_ver)
# 
# 1429:     >>  316 LOAD_GLOBAL          (port)
#              318 LOAD_ATTR            (PID_COMPAY_LIST)
#              320 LOAD_CONST           (10)
#              322 BINARY_SUBSCR
#              324 LOAD_CONST           (2)
#              326 BINARY_SUBSCR
#              328 LOAD_CONST           (1)
#              330 COMPARE_OP           (==)
#              332 EXTENDED_ARG         (256)
#              334 POP_JUMP_IF_FALSE    (to 354)
# 
# 1430:         336 LOAD_GLOBAL          (exe_down_config)
#              338 LOAD_CONST           ("exe_ver_1")
#              340 BINARY_SUBSCR
#              342 STORE_GLOBAL         (exe_ver)
#              344 JUMP_FORWARD         (to 354)
# 
# 1432:     >>  346 LOAD_GLOBAL          (exe_down_config)
#              348 LOAD_CONST           ("exe_ver")
#              350 BINARY_SUBSCR
#              352 STORE_GLOBAL         (exe_ver)
# 
# 1433:     >>  354 LOAD_GLOBAL          (print)
#              356 LOAD_CONST           ("EXE版本号：")
#              358 LOAD_GLOBAL          (exe_ver)
#              360 CALL_FUNCTION        (2 positional arguments)
#              362 POP_TOP
# 
# 1434:         364 LOAD_GLOBAL          (print)
#              366 LOAD_CONST           ("exe_down_config :")
#              368 LOAD_GLOBAL          (exe_down_config)
#              370 FORMAT_VALUE         0
#              372 BUILD_STRING         2
#              374 CALL_FUNCTION        (1 positional argument)
#              376 POP_TOP
# 
# 1435:         378 LOAD_GLOBAL          (compare_versions)
#              380 LOAD_GLOBAL          (exe_ver)
#              382 LOAD_GLOBAL          (port)
#              384 LOAD_ATTR            (Software_version)
#              386 CALL_FUNCTION        (2 positional arguments)
#              388 EXTENDED_ARG         (256)
#              390 POP_JUMP_IF_FALSE    (to 404)
# 
# 1436:         392 LOAD_CONST           (True)
#              394 LOAD_GLOBAL          (port)
#              396 STORE_ATTR           (EXE_updata_flag)
# 
# 1437:         398 POP_BLOCK
#              400 LOAD_CONST           (True)
#              402 RETURN_VALUE
# 
# 1439:     >>  404 LOAD_CONST           (False)
#              406 LOAD_GLOBAL          (port)
#              408 STORE_ATTR           (EXE_updata_flag)
# 
# 1440:         410 POP_BLOCK
#              412 LOAD_CONST           (False)
#              414 RETURN_VALUE
#              416 POP_BLOCK
#              418 JUMP_FORWARD         (to 480)
# 
# 1441:     >>  420 DUP_TOP
#              422 LOAD_GLOBAL          (Exception)
#              424 COMPARE_OP           (exception-match)
#              426 EXTENDED_ARG         (256)
#              428 POP_JUMP_IF_FALSE    (to 478)
#              430 POP_TOP
#              432 STORE_FAST           (e)
#              434 POP_TOP
#              436 SETUP_FINALLY        (to 466)
# 
# 1442:         438 LOAD_GLOBAL          (log)
#              440 LOAD_ATTR            (logger)
#              442 LOAD_METHOD          (error)
#              444 LOAD_CONST           ("检查EXE升级文件JSON下载错误:")
#              446 LOAD_FAST            (e)
#              448 FORMAT_VALUE         0
#              450 BUILD_STRING         2
#              452 CALL_METHOD          (1 positional argument)
#              454 POP_TOP
# 
# 1443:         456 POP_BLOCK
#              458 POP_EXCEPT
#              460 CALL_FINALLY         (to 466)
#              462 LOAD_CONST           (None)
#              464 RETURN_VALUE
#          >>  466 LOAD_CONST           (None)
#              468 STORE_FAST           (e)
#              470 DELETE_FAST          (e)
#              472 END_FINALLY
#              474 POP_EXCEPT
#              476 JUMP_FORWARD         (to 480)
#          >>  478 END_FINALLY
#          >>  480 LOAD_CONST           (None)
#              482 RETURN_VALUE
# 
# 
# ==== .ARGB_new_exe_down1 (line 1447)
# decompile failed (ParserError(SETUP_FINALLY, 0, False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       ARGB_new_exe_down1
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  10
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1447
# # Constants:
# #    0: None
# #    1: 'exe_url'
# #    2: 'ARGB_new_exe_down 上位机下载出现异常'
# #    3: 'Detected proxy:'
# #    4: 'No proxy detected or proxy is disabled.'
# #    5: True
# #    6: 2
# #    7: False
# #    8: ('stream', 'timeout', 'verify')
# #    9: ('stream', 'timeout', 'proxies', 'verify')
# #   10: 'content-length'
# #   11: 0
# #   12: 'wb'
# #   13: 2048
# #   14: ('chunk_size',)
# #   15: 100
# #   16: 'exe下载进度：'
# #   17: 'rb'
# #   18: 'exe_md5'
# #   19: '下载EXE文件校验成功'
# #   20: 'exe 下载异常'
# # Names:
# #    0: exe_down_config
# #    1: log
# #    2: logger
# #    3: error
# #    4: get_windows_proxy
# #    5: print
# #    6: requests
# #    7: get
# #    8: raw
# #    9: decode_content
# #   10: raise_for_status
# #   11: int
# #   12: headers
# #   13: open
# #   14: port
# #   15: file_path_fan_updata_exe
# #   16: iter_content
# #   17: write
# #   18: len
# #   19: loader_progress
# #   20: hashlib
# #   21: md5
# #   22: read
# #   23: hexdigest
# #   24: exe_down_flag
# #   25: Exception
# # Varnames:
# #	url, proxies, prox, response, file_size, downloaded_size, file, data, md5_hash, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: file_size
# #    5: downloaded_size
# #    6: file
# #    7: data
# #    8: md5_hash
# #    9: e
# 
# # Method Name:       ARGB_new_exe_down1
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  10
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1447
# # Constants:
# #    0: None
# #    1: 'exe_url'
# #    2: 'ARGB_new_exe_down 上位机下载出现异常'
# #    3: 'Detected proxy:'
# #    4: 'No proxy detected or proxy is disabled.'
# #    5: True
# #    6: 2
# #    7: False
# #    8: ('stream', 'timeout', 'verify')
# #    9: ('stream', 'timeout', 'proxies', 'verify')
# #   10: 'content-length'
# #   11: 0
# #   12: 'wb'
# #   13: 2048
# #   14: ('chunk_size',)
# #   15: 100
# #   16: 'exe下载进度：'
# #   17: 'rb'
# #   18: 'exe_md5'
# #   19: '下载EXE文件校验成功'
# #   20: 'exe 下载异常'
# # Names:
# #    0: exe_down_config
# #    1: log
# #    2: logger
# #    3: error
# #    4: get_windows_proxy
# #    5: print
# #    6: requests
# #    7: get
# #    8: raw
# #    9: decode_content
# #   10: raise_for_status
# #   11: int
# #   12: headers
# #   13: open
# #   14: port
# #   15: file_path_fan_updata_exe
# #   16: iter_content
# #   17: write
# #   18: len
# #   19: loader_progress
# #   20: hashlib
# #   21: md5
# #   22: read
# #   23: hexdigest
# #   24: exe_down_flag
# #   25: Exception
# # Varnames:
# #	url, proxies, prox, response, file_size, downloaded_size, file, data, md5_hash, e
# # Local variables:
# #    0: url
# #    1: proxies
# #    2: prox
# #    3: response
# #    4: file_size
# #    5: downloaded_size
# #    6: file
# #    7: data
# #    8: md5_hash
# #    9: e
# 1449:           0 SETUP_FINALLY        (to 14)
# 
# 1450:           2 LOAD_GLOBAL          (exe_down_config)
#                4 LOAD_CONST           ("exe_url")
#                6 BINARY_SUBSCR
#                8 STORE_FAST           (url)
#               10 POP_BLOCK
#               12 JUMP_FORWARD         (to 38)
# 
# 1451:     >>   14 POP_TOP
#               16 POP_TOP
#               18 POP_TOP
# 
# 1452:          20 LOAD_GLOBAL          (log)
#               22 LOAD_ATTR            (logger)
#               24 LOAD_METHOD          (error)
#               26 LOAD_CONST           ("ARGB_new_exe_down 上位机下载出现异常")
#               28 CALL_METHOD          (1 positional argument)
#               30 POP_TOP
#               32 POP_EXCEPT
#               34 JUMP_FORWARD         (to 38)
#               36 END_FINALLY
# 
# 1453:     >>   38 EXTENDED_ARG         (256)
#               40 SETUP_FINALLY        (to 322)
# 
# 1454:          42 LOAD_GLOBAL          (get_windows_proxy)
#               44 CALL_FUNCTION        (0 positional arguments)
#               46 STORE_FAST           (proxies)
# 
# 1455:          48 LOAD_FAST            (proxies)
#               50 POP_JUMP_IF_FALSE    (to 64)
# 
# 1456:          52 LOAD_GLOBAL          (print)
#               54 LOAD_CONST           ("Detected proxy:")
#               56 LOAD_FAST            (proxies)
#               58 CALL_FUNCTION        (2 positional arguments)
#               60 POP_TOP
#               62 JUMP_FORWARD         (to 72)
# 
# 1458:     >>   64 LOAD_GLOBAL          (print)
#               66 LOAD_CONST           ("No proxy detected or proxy is disabled.")
#               68 CALL_FUNCTION        (1 positional argument)
#               70 POP_TOP
# 
# 1459:     >>   72 LOAD_GLOBAL          (get_windows_proxy)
#               74 CALL_FUNCTION        (0 positional arguments)
#               76 STORE_FAST           (prox)
# 
# 1460:          78 LOAD_FAST            (prox)
#               80 LOAD_CONST           (None)
#               82 COMPARE_OP           (is)
#               84 POP_JUMP_IF_FALSE    (to 106)
# 
# 1461:          86 LOAD_GLOBAL          (requests)
#               88 LOAD_ATTR            (get)
#               90 LOAD_FAST            (url)
#               92 LOAD_CONST           (True)
#               94 LOAD_CONST           (2)
#               96 LOAD_CONST           (False)
#               98 LOAD_CONST           (('stream', 'timeout', 'verify'))
#              100 CALL_FUNCTION_KW     (4 total positional and keyword args)
#              102 STORE_FAST           (response)
#              104 JUMP_FORWARD         (to 126)
# 
# 1463:     >>  106 LOAD_GLOBAL          (requests)
#              108 LOAD_ATTR            (get)
#              110 LOAD_FAST            (url)
#              112 LOAD_CONST           (True)
#              114 LOAD_CONST           (2)
#              116 LOAD_FAST            (prox)
#              118 LOAD_CONST           (False)
#              120 LOAD_CONST           (('stream', 'timeout', 'proxies', 'verify'))
#              122 CALL_FUNCTION_KW     (5 total positional and keyword args)
#              124 STORE_FAST           (response)
# 
# 1465:     >>  126 LOAD_CONST           (True)
#              128 LOAD_FAST            (response)
#              130 LOAD_ATTR            (raw)
#              132 STORE_ATTR           (decode_content)
# 
# 1466:         134 LOAD_FAST            (response)
#              136 LOAD_METHOD          (raise_for_status)
#              138 CALL_METHOD          (0 positional arguments)
#              140 POP_TOP
# 
# 1467:         142 LOAD_GLOBAL          (int)
#              144 LOAD_FAST            (response)
#              146 LOAD_ATTR            (headers)
#              148 LOAD_METHOD          (get)
#              150 LOAD_CONST           ("content-length")
#              152 LOAD_CONST           (0)
#              154 CALL_METHOD          (2 positional arguments)
#              156 CALL_FUNCTION        (1 positional argument)
#              158 STORE_FAST           (file_size)
# 
# 1468:         160 LOAD_CONST           (0)
#              162 STORE_FAST           (downloaded_size)
# 
# 1469:         164 LOAD_GLOBAL          (open)
#              166 LOAD_GLOBAL          (port)
#              168 LOAD_ATTR            (file_path_fan_updata_exe)
#              170 LOAD_CONST           ("wb")
#              172 CALL_FUNCTION        (2 positional arguments)
#              174 SETUP_WITH           (to 252)
#              176 STORE_FAST           (file)
# 
# 1470:         178 LOAD_FAST            (response)
#              180 LOAD_ATTR            (iter_content)
#              182 LOAD_CONST           (2048)
#              184 LOAD_CONST           (('chunk_size',))
#              186 CALL_FUNCTION_KW     (1 total positional and keyword args)
#              188 GET_ITER
#          >>  190 FOR_ITER             (to 248)
#              192 STORE_FAST           (data)
# 
# 1471:         194 LOAD_FAST            (file)
#              196 LOAD_METHOD          (write)
#              198 LOAD_FAST            (data)
#              200 CALL_METHOD          (1 positional argument)
#              202 POP_TOP
# 
# 1472:         204 LOAD_FAST            (downloaded_size)
#              206 LOAD_GLOBAL          (len)
#              208 LOAD_FAST            (data)
#              210 CALL_FUNCTION        (1 positional argument)
#              212 INPLACE_ADD
#              214 STORE_FAST           (downloaded_size)
# 
# 1473:         216 LOAD_GLOBAL          (int)
#              218 LOAD_FAST            (downloaded_size)
#              220 LOAD_FAST            (file_size)
#              222 BINARY_TRUE_DIVIDE
#              224 LOAD_CONST           (100)
#              226 BINARY_MULTIPLY
#              228 CALL_FUNCTION        (1 positional argument)
#              230 STORE_GLOBAL         (loader_progress)
# 
# 1474:         232 LOAD_GLOBAL          (print)
#              234 LOAD_CONST           ("exe下载进度：")
#              236 LOAD_GLOBAL          (loader_progress)
#              238 FORMAT_VALUE         0
#              240 BUILD_STRING         2
#              242 CALL_FUNCTION        (1 positional argument)
#              244 POP_TOP
#              246 JUMP_ABSOLUTE        (to 190)
#          >>  248 POP_BLOCK
#              250 BEGIN_FINALLY
#          >>  252 WITH_CLEANUP_START
#              254 WITH_CLEANUP_FINISH
#              256 END_FINALLY
# 
# 1475:         258 LOAD_GLOBAL          (hashlib)
#              260 LOAD_METHOD          (md5)
#              262 LOAD_GLOBAL          (open)
#              264 LOAD_GLOBAL          (port)
#              266 LOAD_ATTR            (file_path_fan_updata_exe)
#              268 LOAD_CONST           ("rb")
#              270 CALL_FUNCTION        (2 positional arguments)
#              272 LOAD_METHOD          (read)
#              274 CALL_METHOD          (0 positional arguments)
#              276 CALL_METHOD          (1 positional argument)
#              278 LOAD_METHOD          (hexdigest)
#              280 CALL_METHOD          (0 positional arguments)
#              282 STORE_FAST           (md5_hash)
# 
# 1476:         284 LOAD_FAST            (md5_hash)
#              286 LOAD_GLOBAL          (exe_down_config)
#              288 LOAD_CONST           ("exe_md5")
#              290 BINARY_SUBSCR
#              292 COMPARE_OP           (==)
#              294 EXTENDED_ARG         (256)
#              296 POP_JUMP_IF_FALSE    (to 316)
# 
# 1477:         298 LOAD_GLOBAL          (print)
#              300 LOAD_CONST           ("下载EXE文件校验成功")
#              302 CALL_FUNCTION        (1 positional argument)
#              304 POP_TOP
# 
# 1478:         306 LOAD_CONST           (True)
#              308 STORE_GLOBAL         (exe_down_flag)
# 
# 1479:         310 POP_BLOCK
#              312 LOAD_CONST           (True)
#              314 RETURN_VALUE
# 
# 1480:     >>  316 POP_BLOCK
#              318 LOAD_CONST           (False)
#              320 RETURN_VALUE
# 
# 1481:     >>  322 DUP_TOP
#              324 LOAD_GLOBAL          (Exception)
#              326 COMPARE_OP           (exception-match)
#              328 EXTENDED_ARG         (256)
#              330 POP_JUMP_IF_FALSE    (to 376)
#              332 POP_TOP
#              334 STORE_FAST           (e)
#              336 POP_TOP
#              338 SETUP_FINALLY        (to 364)
# 
# 1482:         340 LOAD_GLOBAL          (print)
#              342 LOAD_CONST           ("exe 下载异常")
#              344 LOAD_FAST            (e)
#              346 FORMAT_VALUE         0
#              348 BUILD_STRING         2
#              350 CALL_FUNCTION        (1 positional argument)
#              352 POP_TOP
# 
# 1483:         354 POP_BLOCK
#              356 POP_EXCEPT
#              358 CALL_FINALLY         (to 364)
#              360 LOAD_CONST           (False)
#              362 RETURN_VALUE
#          >>  364 LOAD_CONST           (None)
#              366 STORE_FAST           (e)
#              368 DELETE_FAST          (e)
#              370 END_FINALLY
#              372 POP_EXCEPT
#              374 JUMP_FORWARD         (to 378)
#          >>  376 END_FINALLY
#          >>  378 LOAD_CONST           (None)
#              380 RETURN_VALUE
# 
# 
# ==== .ARGB_new_exe_down (line 1488)
# decompile failed (ParserError(LOAD_CONST, 0, False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       ARGB_new_exe_down
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  15
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1488
# # Constants:
# #    0: None
# #    1: False
# #    2: 0
# #    3: 3
# #    4: 'exe_url'
# #    5: 'ARGB_new_exe_down 上位机下载出现异常'
# #    6: 'Detected proxy:'
# #    7: 'No proxy detected or proxy is disabled.'
# #    8: (2, 5)
# #    9: True
# #   10: ('stream', 'timeout', 'verify')
# #   11: ('stream', 'timeout', 'proxies', 'verify')
# #   12: 'content-length'
# #   13: 'wb'
# #   14: 2048
# #   15: ('chunk_size',)
# #   16: 100
# #   17: 'exe下载进度：'
# #   18: 'rb'
# #   19: 8192
# #   20: 'exe_md5'
# #   21: '下载EXE文件校验成功'
# #   22: 'MD5校验不一致，准备重试'
# #   23: 'MD5校验失败'
# #   24: 1
# #   25: 'exe 下载异常'
# #   26: '，全局重试 '
# #   27: '/'
# #   28: 255
# #   29: '已耗尽'
# #   30: '次重试，下载最终失败'
# # Names:
# #    0: exe_down_flag
# #    1: loader_progress
# #    2: port
# #    3: file_path_fan_updata_exe
# #    4: try_count
# #    5: exe_down_config
# #    6: log
# #    7: logger
# #    8: error
# #    9: get_windows_proxy
# #   10: print
# #   11: requests
# #   12: get
# #   13: raw
# #   14: decode_content
# #   15: raise_for_status
# #   16: int
# #   17: headers
# #   18: open
# #   19: iter_content
# #   20: write
# #   21: len
# #   22: hashlib
# #   23: md5
# #   24: read
# #   25: update
# #   26: hexdigest
# #   27: Exception
# #   28: exe_down_done
# # Varnames:
# #	save_path, max_retry, url, proxies, prox, timeout_set, response, file_size, downloaded_size, file, data, md5_hash, f, chunk, e
# # Local variables:
# #    0: save_path
# #    1: max_retry
# #    2: url
# #    3: proxies
# #    4: prox
# #    5: timeout_set
# #    6: response
# #    7: file_size
# #    8: downloaded_size
# #    9: file
# #   10: data
# #   11: md5_hash
# #   12: f
# #   13: chunk
# #   14: e
# 
# # Method Name:       ARGB_new_exe_down
# # Filename:          agreement.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  15
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1488
# # Constants:
# #    0: None
# #    1: False
# #    2: 0
# #    3: 3
# #    4: 'exe_url'
# #    5: 'ARGB_new_exe_down 上位机下载出现异常'
# #    6: 'Detected proxy:'
# #    7: 'No proxy detected or proxy is disabled.'
# #    8: (2, 5)
# #    9: True
# #   10: ('stream', 'timeout', 'verify')
# #   11: ('stream', 'timeout', 'proxies', 'verify')
# #   12: 'content-length'
# #   13: 'wb'
# #   14: 2048
# #   15: ('chunk_size',)
# #   16: 100
# #   17: 'exe下载进度：'
# #   18: 'rb'
# #   19: 8192
# #   20: 'exe_md5'
# #   21: '下载EXE文件校验成功'
# #   22: 'MD5校验不一致，准备重试'
# #   23: 'MD5校验失败'
# #   24: 1
# #   25: 'exe 下载异常'
# #   26: '，全局重试 '
# #   27: '/'
# #   28: 255
# #   29: '已耗尽'
# #   30: '次重试，下载最终失败'
# # Names:
# #    0: exe_down_flag
# #    1: loader_progress
# #    2: port
# #    3: file_path_fan_updata_exe
# #    4: try_count
# #    5: exe_down_config
# #    6: log
# #    7: logger
# #    8: error
# #    9: get_windows_proxy
# #   10: print
# #   11: requests
# #   12: get
# #   13: raw
# #   14: decode_content
# #   15: raise_for_status
# #   16: int
# #   17: headers
# #   18: open
# #   19: iter_content
# #   20: write
# #   21: len
# #   22: hashlib
# #   23: md5
# #   24: read
# #   25: update
# #   26: hexdigest
# #   27: Exception
# #   28: exe_down_done
# # Varnames:
# #	save_path, max_retry, url, proxies, prox, timeout_set, response, file_size, downloaded_size, file, data, md5_hash, f, chunk, e
# # Local variables:
# #    0: save_path
# #    1: max_retry
# #    2: url
# #    3: proxies
# #    4: prox
# #    5: timeout_set
# #    6: response
# #    7: file_size
# #    8: downloaded_size
# #    9: file
# #   10: data
# #   11: md5_hash
# #   12: f
# #   13: chunk
# #   14: e
# 1490:           0 LOAD_CONST           (False)
#                2 STORE_GLOBAL         (exe_down_flag)
# 
# 1491:           4 LOAD_CONST           (0)
#                6 STORE_GLOBAL         (loader_progress)
# 
# 1492:           8 LOAD_GLOBAL          (port)
#               10 LOAD_ATTR            (file_path_fan_updata_exe)
#               12 STORE_FAST           (save_path)
# 
# 1493:          14 LOAD_CONST           (3)
#               16 STORE_FAST           (max_retry)
# 
# 1496:     >>   18 LOAD_GLOBAL          (try_count)
#               20 LOAD_FAST            (max_retry)
#               22 COMPARE_OP           (<)
#               24 EXTENDED_ARG         (256)
#               26 POP_JUMP_IF_FALSE    (to 510)
# 
# 1497:          28 SETUP_FINALLY        (to 42)
# 
# 1498:          30 LOAD_GLOBAL          (exe_down_config)
#               32 LOAD_CONST           ("exe_url")
#               34 BINARY_SUBSCR
#               36 STORE_FAST           (url)
#               38 POP_BLOCK
#               40 JUMP_FORWARD         (to 68)
# 
# 1499:     >>   42 POP_TOP
#               44 POP_TOP
#               46 POP_TOP
# 
# 1500:          48 LOAD_GLOBAL          (log)
#               50 LOAD_ATTR            (logger)
#               52 LOAD_METHOD          (error)
#               54 LOAD_CONST           ("ARGB_new_exe_down 上位机下载出现异常")
#               56 CALL_METHOD          (1 positional argument)
#               58 POP_TOP
# 
# 1501:          60 POP_EXCEPT
#               62 LOAD_CONST           (False)
#               64 RETURN_VALUE
#               66 END_FINALLY
# 
# 1502:     >>   68 EXTENDED_ARG         (256)
#               70 SETUP_FINALLY        (to 420)
# 
# 1503:          72 LOAD_GLOBAL          (get_windows_proxy)
#               74 CALL_FUNCTION        (0 positional arguments)
#               76 STORE_FAST           (proxies)
# 
# 1504:          78 LOAD_FAST            (proxies)
#               80 POP_JUMP_IF_FALSE    (to 94)
# 
# 1505:          82 LOAD_GLOBAL          (print)
#               84 LOAD_CONST           ("Detected proxy:")
#               86 LOAD_FAST            (proxies)
#               88 CALL_FUNCTION        (2 positional arguments)
#               90 POP_TOP
#               92 JUMP_FORWARD         (to 102)
# 
# 1507:     >>   94 LOAD_GLOBAL          (print)
#               96 LOAD_CONST           ("No proxy detected or proxy is disabled.")
#               98 CALL_FUNCTION        (1 positional argument)
#              100 POP_TOP
# 
# 1508:     >>  102 LOAD_GLOBAL          (get_windows_proxy)
#              104 CALL_FUNCTION        (0 positional arguments)
#              106 STORE_FAST           (prox)
# 
# 1510:         108 LOAD_CONST           ((2, 5))
#              110 STORE_FAST           (timeout_set)
# 
# 1511:         112 LOAD_FAST            (prox)
#              114 LOAD_CONST           (None)
#              116 COMPARE_OP           (is)
#              118 POP_JUMP_IF_FALSE    (to 140)
# 
# 1512:         120 LOAD_GLOBAL          (requests)
#              122 LOAD_ATTR            (get)
#              124 LOAD_FAST            (url)
#              126 LOAD_CONST           (True)
#              128 LOAD_FAST            (timeout_set)
#              130 LOAD_CONST           (False)
#              132 LOAD_CONST           (('stream', 'timeout', 'verify'))
#              134 CALL_FUNCTION_KW     (4 total positional and keyword args)
#              136 STORE_FAST           (response)
#              138 JUMP_FORWARD         (to 160)
# 
# 1514:     >>  140 LOAD_GLOBAL          (requests)
#              142 LOAD_ATTR            (get)
#              144 LOAD_FAST            (url)
#              146 LOAD_CONST           (True)
#              148 LOAD_FAST            (timeout_set)
#              150 LOAD_FAST            (prox)
#              152 LOAD_CONST           (False)
#              154 LOAD_CONST           (('stream', 'timeout', 'proxies', 'verify'))
#              156 CALL_FUNCTION_KW     (5 total positional and keyword args)
#              158 STORE_FAST           (response)
# 
# 1516:     >>  160 LOAD_CONST           (True)
#              162 LOAD_FAST            (response)
#              164 LOAD_ATTR            (raw)
#              166 STORE_ATTR           (decode_content)
# 
# 1517:         168 LOAD_FAST            (response)
#              170 LOAD_METHOD          (raise_for_status)
#              172 CALL_METHOD          (0 positional arguments)
#              174 POP_TOP
# 
# 1518:         176 LOAD_GLOBAL          (int)
#              178 LOAD_FAST            (response)
#              180 LOAD_ATTR            (headers)
#              182 LOAD_METHOD          (get)
#              184 LOAD_CONST           ("content-length")
#              186 LOAD_CONST           (0)
#              188 CALL_METHOD          (2 positional arguments)
#              190 CALL_FUNCTION        (1 positional argument)
#              192 STORE_FAST           (file_size)
# 
# 1519:         194 LOAD_CONST           (0)
#              196 STORE_FAST           (downloaded_size)
# 
# 1520:         198 LOAD_GLOBAL          (open)
#              200 LOAD_GLOBAL          (port)
#              202 LOAD_ATTR            (file_path_fan_updata_exe)
#              204 LOAD_CONST           ("wb")
#              206 CALL_FUNCTION        (2 positional arguments)
#              208 SETUP_WITH           (to 294)
#              210 STORE_FAST           (file)
# 
# 1521:         212 LOAD_FAST            (response)
#              214 LOAD_ATTR            (iter_content)
#              216 LOAD_CONST           (2048)
#              218 LOAD_CONST           (('chunk_size',))
#              220 CALL_FUNCTION_KW     (1 total positional and keyword args)
#              222 GET_ITER
#          >>  224 FOR_ITER             (to 290)
#              226 STORE_FAST           (data)
# 
# 1522:         228 LOAD_FAST            (file)
#              230 LOAD_METHOD          (write)
#              232 LOAD_FAST            (data)
#              234 CALL_METHOD          (1 positional argument)
#              236 POP_TOP
# 
# 1523:         238 LOAD_FAST            (downloaded_size)
#              240 LOAD_GLOBAL          (len)
#              242 LOAD_FAST            (data)
#              244 CALL_FUNCTION        (1 positional argument)
#              246 INPLACE_ADD
#              248 STORE_FAST           (downloaded_size)
# 
# 1525:         250 LOAD_FAST            (file_size)
#              252 LOAD_CONST           (0)
#              254 COMPARE_OP           (>)
#              256 POP_JUMP_IF_FALSE    (to 224)
# 
# 1526:         258 LOAD_GLOBAL          (int)
#              260 LOAD_FAST            (downloaded_size)
#              262 LOAD_FAST            (file_size)
#              264 BINARY_TRUE_DIVIDE
#              266 LOAD_CONST           (100)
#              268 BINARY_MULTIPLY
#              270 CALL_FUNCTION        (1 positional argument)
#              272 STORE_GLOBAL         (loader_progress)
# 
# 1527:         274 LOAD_GLOBAL          (print)
#              276 LOAD_CONST           ("exe下载进度：")
#              278 LOAD_GLOBAL          (loader_progress)
#              280 FORMAT_VALUE         0
#              282 BUILD_STRING         2
#              284 CALL_FUNCTION        (1 positional argument)
#              286 POP_TOP
#              288 JUMP_ABSOLUTE        (to 224)
#          >>  290 POP_BLOCK
#              292 BEGIN_FINALLY
#          >>  294 WITH_CLEANUP_START
#              296 WITH_CLEANUP_FINISH
#              298 END_FINALLY
# 
# 1529:         300 LOAD_GLOBAL          (hashlib)
#              302 LOAD_METHOD          (md5)
#              304 CALL_METHOD          (0 positional arguments)
#              306 STORE_FAST           (md5_hash)
# 
# 1530:         308 LOAD_GLOBAL          (open)
#              310 LOAD_FAST            (save_path)
#              312 LOAD_CONST           ("rb")
#              314 CALL_FUNCTION        (2 positional arguments)
#              316 SETUP_WITH           (to 354)
#              318 STORE_FAST           (f)
# 
# 1531:     >>  320 LOAD_FAST            (f)
#              322 LOAD_METHOD          (read)
#              324 LOAD_CONST           (8192)
#              326 CALL_METHOD          (1 positional argument)
#              328 DUP_TOP
#              330 STORE_FAST           (chunk)
#              332 EXTENDED_ARG         (256)
#              334 POP_JUMP_IF_FALSE    (to 350)
# 
# 1532:         336 LOAD_FAST            (md5_hash)
#              338 LOAD_METHOD          (update)
#              340 LOAD_FAST            (chunk)
#              342 CALL_METHOD          (1 positional argument)
#              344 POP_TOP
#              346 EXTENDED_ARG         (256)
#              348 JUMP_ABSOLUTE        (to 320)
#          >>  350 POP_BLOCK
#              352 BEGIN_FINALLY
#          >>  354 WITH_CLEANUP_START
#              356 WITH_CLEANUP_FINISH
#              358 END_FINALLY
# 
# 1533:         360 LOAD_FAST            (md5_hash)
#              362 LOAD_METHOD          (hexdigest)
#              364 CALL_METHOD          (0 positional arguments)
#              366 STORE_FAST           (md5_hash)
# 
# 1534:         368 LOAD_FAST            (md5_hash)
#              370 LOAD_GLOBAL          (exe_down_config)
#              372 LOAD_CONST           ("exe_md5")
#              374 BINARY_SUBSCR
#              376 COMPARE_OP           (==)
#              378 EXTENDED_ARG         (256)
#              380 POP_JUMP_IF_FALSE    (to 400)
# 
# 1535:         382 LOAD_GLOBAL          (print)
#              384 LOAD_CONST           ("下载EXE文件校验成功")
#              386 CALL_FUNCTION        (1 positional argument)
#              388 POP_TOP
# 
# 1536:         390 LOAD_CONST           (True)
#              392 STORE_GLOBAL         (exe_down_flag)
# 
# 1537:         394 POP_BLOCK
#              396 LOAD_CONST           (True)
#              398 RETURN_VALUE
# 
# 1539:     >>  400 LOAD_GLOBAL          (print)
#              402 LOAD_CONST           ("MD5校验不一致，准备重试")
#              404 CALL_FUNCTION        (1 positional argument)
#              406 POP_TOP
# 
# 1540:         408 LOAD_GLOBAL          (Exception)
#              410 LOAD_CONST           ("MD5校验失败")
#              412 CALL_FUNCTION        (1 positional argument)
#              414 RAISE_VARARGS        (exception instance)
#              416 POP_BLOCK
#              418 JUMP_ABSOLUTE        (to 18)
# 
# 1541:     >>  420 DUP_TOP
#              422 LOAD_GLOBAL          (Exception)
#              424 COMPARE_OP           (exception-match)
#              426 EXTENDED_ARG         (256)
#              428 POP_JUMP_IF_FALSE    (to 506)
#              430 POP_TOP
#              432 STORE_FAST           (e)
#              434 POP_TOP
#              436 SETUP_FINALLY        (to 494)
# 
# 1542:         438 LOAD_GLOBAL          (try_count)
#              440 LOAD_CONST           (1)
#              442 INPLACE_ADD
#              444 STORE_GLOBAL         (try_count)
# 
# 1543:         446 LOAD_GLOBAL          (print)
#              448 LOAD_CONST           ("exe 下载异常")
#              450 LOAD_FAST            (e)
#              452 FORMAT_VALUE         0
#              454 LOAD_CONST           ("，全局重试 ")
#              456 LOAD_GLOBAL          (try_count)
#              458 FORMAT_VALUE         0
#              460 LOAD_CONST           ("/")
#              462 LOAD_FAST            (max_retry)
#              464 FORMAT_VALUE         0
#              466 BUILD_STRING         6
#              468 CALL_FUNCTION        (1 positional argument)
#              470 POP_TOP
# 
# 1544:         472 LOAD_GLOBAL          (log)
#              474 LOAD_ATTR            (logger)
#              476 LOAD_METHOD          (error)
#              478 LOAD_CONST           ("exe 下载异常")
#              480 LOAD_FAST            (e)
#              482 FORMAT_VALUE         0
#              484 BUILD_STRING         2
#              486 CALL_METHOD          (1 positional argument)
#              488 POP_TOP
#              490 POP_BLOCK
#              492 BEGIN_FINALLY
#          >>  494 LOAD_CONST           (None)
#              496 STORE_FAST           (e)
#              498 DELETE_FAST          (e)
#              500 END_FINALLY
#              502 POP_EXCEPT
#              504 JUMP_ABSOLUTE        (to 18)
#          >>  506 END_FINALLY
#              508 JUMP_ABSOLUTE        (to 18)
# 
# 1553:     >>  510 LOAD_CONST           (255)
#              512 STORE_GLOBAL         (exe_down_done)
# 
# 1554:         514 LOAD_GLOBAL          (print)
#              516 LOAD_CONST           ("已耗尽")
#              518 LOAD_FAST            (max_retry)
#              520 FORMAT_VALUE         0
#              522 LOAD_CONST           ("次重试，下载最终失败")
#              524 BUILD_STRING         3
#              526 CALL_FUNCTION        (1 positional argument)
#              528 POP_TOP
# 
# 1555:         530 LOAD_CONST           (False)
#              532 RETURN_VALUE
# 
# 
# ==== .argb_fan_read_config (line 1596)
# decompile failed (ParserError(COLLECTION_START, '0_0', False)); disassembly:
# # pydisasm version 6.1.7
# # Python bytecode 3.8 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       argb_fan_read_config
# # Filename:          agreement.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  5
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1596
# # Constants:
# #    0: None
# #    1: 0
# #    2: 82
# #    3: 66
# #    4: 7
# #    5: 1
# #    6: 255
# #    7: 256
# #    8: 64
# #    9: 1500
# #   10: ('timeout_ms',)
# #   11: '获取10路设备信息：'
# #   12: 4
# #   13: 6
# #   14: 'RGB风扇 获取所有设备信息异常: '
# # Names:
# #    0: sum
# #    1: append
# #    2: clear_buf
# #    3: write
# #    4: print
# #    5: read
# #    6: Exception
# # Varnames:
# #	ser, send_hex, send_hex_sum, data, e
# # Positional arguments:
# #	ser
# # Local variables:
# #    1: send_hex
# #    2: send_hex_sum
# #    3: data
# #    4: e
# 
# # Method Name:       argb_fan_read_config
# # Filename:          agreement.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  5
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        1596
# # Constants:
# #    0: None
# #    1: 0
# #    2: 82
# #    3: 66
# #    4: 7
# #    5: 1
# #    6: 255
# #    7: 256
# #    8: 64
# #    9: 1500
# #   10: ('timeout_ms',)
# #   11: '获取10路设备信息：'
# #   12: 4
# #   13: 6
# #   14: 'RGB风扇 获取所有设备信息异常: '
# # Names:
# #    0: sum
# #    1: append
# #    2: clear_buf
# #    3: write
# #    4: print
# #    5: read
# #    6: Exception
# # Varnames:
# #	ser, send_hex, send_hex_sum, data, e
# # Positional arguments:
# #	ser
# # Local variables:
# #    1: send_hex
# #    2: send_hex_sum
# #    3: data
# #    4: e
# 1597:           0 LOAD_CONST           (0)
#                2 LOAD_CONST           (82)
#                4 LOAD_CONST           (66)
#                6 LOAD_CONST           (7)
#                8 LOAD_CONST           (0)
#               10 LOAD_CONST           (1)
#               12 LOAD_CONST           (255)
#               14 BUILD_LIST           7
#               16 STORE_FAST           (send_hex)
# 
# 1598:          18 LOAD_GLOBAL          (sum)
#               20 LOAD_FAST            (send_hex)
#               22 CALL_FUNCTION        (1 positional argument)
#               24 LOAD_CONST           (256)
#               26 BINARY_MODULO
#               28 STORE_FAST           (send_hex_sum)
# 
# 1599:          30 LOAD_FAST            (send_hex)
#               32 LOAD_METHOD          (append)
#               34 LOAD_FAST            (send_hex_sum)
#               36 CALL_METHOD          (1 positional argument)
#               38 POP_TOP
# 
# 1600:          40 SETUP_FINALLY        (to 140)
# 
# 1601:          42 LOAD_GLOBAL          (clear_buf)
#               44 LOAD_FAST            (ser)
#               46 CALL_FUNCTION        (1 positional argument)
#               48 POP_TOP
# 
# 1602:          50 LOAD_FAST            (ser)
#               52 LOAD_METHOD          (write)
#               54 LOAD_FAST            (send_hex)
#               56 CALL_METHOD          (1 positional argument)
#               58 POP_TOP
# 
# 1603:          60 LOAD_GLOBAL          (print)
#               62 LOAD_FAST            (send_hex)
#               64 CALL_FUNCTION        (1 positional argument)
#               66 POP_TOP
# 
# 1604:          68 LOAD_FAST            (ser)
#               70 LOAD_ATTR            (read)
#               72 LOAD_CONST           (64)
#               74 LOAD_CONST           (1500)
#               76 LOAD_CONST           (('timeout_ms',))
#               78 CALL_FUNCTION_KW     (2 total positional and keyword args)
#               80 STORE_FAST           (data)
# 
# 1605:          82 LOAD_GLOBAL          (print)
#               84 LOAD_CONST           ("获取10路设备信息：")
#               86 LOAD_FAST            (data)
#               88 FORMAT_VALUE         0
#               90 BUILD_STRING         2
#               92 CALL_FUNCTION        (1 positional argument)
#               94 POP_TOP
# 
# 1606:          96 LOAD_FAST            (data)
#               98 BUILD_LIST           0
#              100 COMPARE_OP           (!=)
#              102 POP_JUMP_IF_FALSE    (to 130)
#              104 LOAD_FAST            (data)
#              106 LOAD_CONST           (4)
#              108 BINARY_SUBSCR
#              110 LOAD_CONST           (1)
#              112 COMPARE_OP           (==)
#              114 POP_JUMP_IF_FALSE    (to 130)
# 
# 1607:         116 LOAD_FAST            (data)
#              118 LOAD_CONST           (6)
#              120 LOAD_CONST           (64)
#              122 BUILD_SLICE          2
#              124 BINARY_SUBSCR
#              126 POP_BLOCK
#              128 RETURN_VALUE
# 
# 1609:     >>  130 POP_BLOCK
#              132 LOAD_CONST           (None)
#              134 RETURN_VALUE
#              136 POP_BLOCK
#              138 JUMP_FORWARD         (to 194)
# 
# 1610:     >>  140 DUP_TOP
#              142 LOAD_GLOBAL          (Exception)
#              144 COMPARE_OP           (exception-match)
#              146 POP_JUMP_IF_FALSE    (to 192)
#              148 POP_TOP
#              150 STORE_FAST           (e)
#              152 POP_TOP
#              154 SETUP_FINALLY        (to 180)
# 
# 1611:         156 LOAD_GLOBAL          (print)
#              158 LOAD_CONST           ("RGB风扇 获取所有设备信息异常: ")
#              160 LOAD_FAST            (e)
#              162 FORMAT_VALUE         0
#              164 BUILD_STRING         2
#              166 CALL_FUNCTION        (1 positional argument)
#              168 POP_TOP
# 
# 1612:         170 POP_BLOCK
#              172 POP_EXCEPT
#              174 CALL_FINALLY         (to 180)
#              176 LOAD_CONST           (None)
#              178 RETURN_VALUE
#          >>  180 LOAD_CONST           (None)
#              182 STORE_FAST           (e)
#              184 DELETE_FAST          (e)
#              186 END_FINALLY
#              188 POP_EXCEPT
#              190 JUMP_FORWARD         (to 194)
#          >>  192 END_FINALLY
#          >>  194 LOAD_CONST           (None)
#              196 RETURN_VALUE
# 
# 
# ==== .argb_fan_set (line 1614)
send_hex = [
 0,82,66,8,0,5,id,1]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    print(f"设备ID {id}")
    ser.write(send_hex)
    print(f"agreement argb_fan_set(ser, id) 设置风扇数据：{send_hex}")
except Exception as e:
    try:
        print(f"单设备灯效指令异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_fan_light_off (line 1627)
send_hex = [
 0,82,66,6,0,6]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"ARGB控制器关灯指令：{send_hex}")
except Exception as e:
    try:
        print(f"ARGB控制器关灯指令：异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_fan_reset (line 1638)
send_hex = [
 0,82,66,6,0,250]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"重置ARGB控制器指令：{send_hex}")
except Exception as e:
    try:
        print(f"重置ARGB控制器指令：异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_fan_save (line 1649)
send_hex = [
 0,82,66,6,0,252]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"ARGB控制器重启：{send_hex}")
except Exception as e:
    try:
        print(f"ARGB控制器重启：异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_fan_save_effect (line 1660)
send_hex = [
 0,82,66,6,0,249]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"ARGB控制器保存灯效：{send_hex}")
except Exception as e:
    try:
        print(f"ARGB控制器保存灯效：异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_auto_rgb_num (line 1671)
send_hex = [
 0,82,66,6,0,7]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"自识别灯珠数：{send_hex}")
except Exception as e:
    try:
        print(f"自识别灯珠数：异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_cp_mcu_fan_speed (line 1682)
send_hex = [
 0,82,66,7,0,8,num]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"风扇转速控制权：{send_hex}")
except Exception as e:
    try:
        print(f"风扇转速控制权异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_433peidui (line 1693)
send_hex = [
 0,82,66,6,0,9]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"遥控器进入配对模式：{send_hex}")
except Exception as e:
    try:
        print(f"遥控器进入配对模式异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_433qingchupeidui (line 1704)
send_hex = [
 0,82,66,6,0,10]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"遥控器清除配对：{send_hex}")
except Exception as e:
    try:
        print(f"遥控器清除配对异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_jiyi_on_effect (line 1715)
send_hex = [
 0,82,66,7,0,11,vaule]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"开机灯效模式设置：{send_hex}")
except Exception as e:
    try:
        print(f"开机灯效模式设置异常: {e}")
    finally:
        e = None
        del e
# ==== .argb_on_off_kaijizijian (line 1730)
send_hex = [
 0,82,66,7,0,12,vaule]
send_hex_sum = sum(send_hex) % 256
send_hex.append(send_hex_sum)
try:
    ser.write(send_hex)
    print(f"开机自检设置：{send_hex}")
except Exception as e:
    try:
        print(f"开机自检设置异常: {e}")
    finally:
        e = None
        del e