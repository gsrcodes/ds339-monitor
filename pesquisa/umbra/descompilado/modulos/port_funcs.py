
# ==== .init_config (line 598)
log.logger.info("软件打开，进入初始化流程")
log.logger.info(f"软件名称：Sync_light， 软件版本：{Software_version}， 软件编译日期：{Year_data}.{Compilation_date}")
if not os.path.exists(file_path_template_ble):
    with open(file_path_template_ble, "w", encoding="utf-8") as f:
        json.dump((json_init.init_config_data), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_path_template_ble}")
else:
    print(f"文件 {file_path_template_ble} 已存在，不会写入新数据。")
    try:
        with open(file_path_template_ble, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.init_config_data["ver"]:
                with open(file_path_template_ble, "w", encoding="utf-8") as f:
                    json.dump((json_init.init_config_data), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_path_template_ble}")
    except:
        print("打开产品配置文件出错")
        with open(file_path_template_ble, "w", encoding="utf-8") as f:
            json.dump((json_init.init_config_data), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_path_template_ble}")

if not os.path.exists(file_path_ble):
    with open(file_path_ble, "w", encoding="utf-8") as f:
        json.dump((json_init.init_data), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_path_ble}")
else:
    print(f"文件 {file_path_ble} 已存在，不会写入新数据。")
    try:
        with open(file_path_ble, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.init_data["ver"]:
                with open(file_path_ble, "w", encoding="utf-8") as f:
                    json.dump((json_init.init_data), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_path_ble}")
    except:
        print("打开产品配置文件出错")
        with open(file_path_ble, "w", encoding="utf-8") as f:
            json.dump((json_init.init_data), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_path_ble}")

if not os.path.exists(file_path_device_config_ble):
    with open(file_path_device_config_ble, "w", encoding="utf-8") as f:
        json.dump((json_init.init_device_rgb), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_path_device_config_ble}")
else:
    print(f"文件 {file_path_device_config_ble} 已存在，不会写入新数据。")
    try:
        with open(file_path_device_config_ble, "r", encoding="utf-8") as file:
            data = json.load(file)
    except:
        print("打开产品配置文件出错")
        with open(file_path_device_config_ble, "w", encoding="utf-8") as f:
            json.dump((json_init.init_device_rgb), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_path_device_config_ble}")

if not os.path.exists(file_path_template_usb_hid):
    with open(file_path_template_usb_hid, "w", encoding="utf-8") as f:
        json.dump((json_init.init_yuan_usb_hid), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_path_template_usb_hid}")
else:
    print(f"文件 {file_path_template_usb_hid} 已存在，不会写入新数据。")
    try:
        with open(file_path_template_usb_hid, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.init_yuan_usb_hid["ver"]:
                with open(file_path_template_usb_hid, "w", encoding="utf-8") as f:
                    json.dump((json_init.init_yuan_usb_hid), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_path_template_usb_hid}")
    except:
        print("打开产品配置文件出错")
        with open(file_path_template_usb_hid, "w", encoding="utf-8") as f:
            json.dump((json_init.init_yuan_usb_hid), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_path_template_usb_hid}")

if not os.path.exists(file_path_usb_hid):
    with open(file_path_usb_hid, "w", encoding="utf-8") as f:
        json.dump((json_init.data_usb_hid), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_path_usb_hid}")
else:
    print(f"文件 {file_path_usb_hid} 已存在，不会写入新数据。")
    try:
        with open(file_path_usb_hid, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.data_usb_hid["ver"]:
                with open(file_path_usb_hid, "w", encoding="utf-8") as f:
                    json.dump((json_init.data_usb_hid), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_path_usb_hid}")
    except:
        print("打开产品配置文件出错")
        with open(file_path_usb_hid, "w", encoding="utf-8") as f:
            json.dump((json_init.data_usb_hid), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_path_usb_hid}")

if not os.path.exists(file_path_fan_config):
    with open(file_path_fan_config, "w", encoding="utf-8") as f:
        json.dump((json_init.ARGB), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_path_fan_config}")
else:
    print(f"文件 {file_path_fan_config} 已存在，不会写入新数据。")
    try:
        with open(file_path_fan_config, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.ARGB["ver"]:
                with open(file_path_fan_config, "w", encoding="utf-8") as f:
                    json.dump((json_init.ARGB), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_path_fan_config}")
    except:
        print("打开产品配置文件出错")
        with open(file_path_fan_config, "w", encoding="utf-8") as f:
            json.dump((json_init.ARGB), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_path_fan_config}")

if not os.path.exists(file_argb_jiyi_config):
    with open(file_argb_jiyi_config, "w", encoding="utf-8") as f:
        json.dump((json_init.ARGB_EFFECT_INIT), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_argb_jiyi_config}")
else:
    print(f"文件 {file_argb_jiyi_config} 已存在，不会写入新数据。")
    try:
        with open(file_argb_jiyi_config, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] != json_init.ARGB_EFFECT_INIT["ver"]:
                with open(file_argb_jiyi_config, "w", encoding="utf-8") as f:
                    json.dump((json_init.ARGB_EFFECT_INIT), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_argb_jiyi_config}")
    except:
        print("打开产品配置文件出错")
        with open(file_argb_jiyi_config, "w", encoding="utf-8") as f:
            json.dump((json_init.ARGB_EFFECT_INIT), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_argb_jiyi_config}")

if not os.path.exists(file_exe_updata):
    with open(file_exe_updata, "w", encoding="utf-8") as f:
        json.dump((json_init.exe_uploader), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_exe_updata}")
else:
    print(f"文件 {file_exe_updata} 已存在，不会写入新数据。")
    try:
        with open(file_exe_updata, "r", encoding="utf-8") as file:
            pass
    except:
        print("打开exe升级文件JSON出错")
        with open(file_exe_updata, "w", encoding="utf-8") as f:
            json.dump((json_init.exe_uploader), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_exe_updata}")

if not os.path.exists(file_argb_neiwang_config):
    with open(file_argb_neiwang_config, "w", encoding="utf-8") as f:
        json.dump((json_init.ARGB_UPLOADER_NEIWANG), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_argb_neiwang_config}")
else:
    print(f"文件 {file_argb_neiwang_config} 已存在，不会写入新数据。")
    try:
        with open(file_argb_neiwang_config, "r", encoding="utf-8") as file:
            data = json.load(file)
            if data["ver"] < json_init.ARGB_UPLOADER_NEIWANG["ver"]:
                with open(file_argb_neiwang_config, "w", encoding="utf-8") as f:
                    json.dump((json_init.ARGB_UPLOADER_NEIWANG), f, ensure_ascii=False, indent=4)
                print(f"JSON 数据已成功写入到 {file_argb_neiwang_config}")
    except:
        print("打开exe升级文件JSON出错")
        with open(file_argb_neiwang_config, "w", encoding="utf-8") as f:
            json.dump((json_init.ARGB_UPLOADER_NEIWANG), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_argb_neiwang_config}")

if not os.path.exists(file_argb_terminal):
    with open(file_argb_terminal, "w", encoding="utf-8") as f:
        json.dump((json_init.ARGB_TERMINAL), f, ensure_ascii=False, indent=4)
    print(f"JSON 数据已成功写入到 {file_argb_terminal}")
else:
    print(f"文件 {file_argb_terminal} 已存在，不会写入新数据。")
    try:
        with open(file_argb_terminal, "r", encoding="utf-8") as file:
            pass
    except:
        print("打开终端激活文件JSON出错")
        with open(file_argb_terminal, "w", encoding="utf-8") as f:
            json.dump((json_init.ARGB_TERMINAL), f, ensure_ascii=False, indent=4)
        print(f"JSON 数据已成功写入到 {file_argb_terminal}")

tp = config_tp()
ON = config_ON_language()[0]
language = config_ON_language()[1]
print(f"当前语言：{language}")
minimize = config_ON_language()[2]
Light_mode = config_ON_language()[3]
renwulan_index = config_ON_language()[4]
diannao_shiyin_yanse = config_read_diannao_shiyin()
device_state = config_device_state()
ARGB_TEN, ARGB_FAN_config, ARGB_dan_config, ARGB_dan_sync_config_chuji, ARGB_dan_sync_config_gaoji, ARGB_qun_async_config = read_argb_fan_all()
fan_argb_button_model_index, gaoji_ten_dengxiao_leixing, ARGB_TEN_MODEL1, fan_basic_effect, fan_advanced_effect, MCU_C_FLAG, FAN_speed, HEX_UPLOADER_ERROR, GONG_NEI, On_Effect = argb_jiyi_read()
ARGB_neiwang_hex, ARGB_neiwang_exe, ARGB_neiwang = argb_neiwang_read()
if fan_advanced_effect[12] == 0 or language == 0:
    language_index()
    print("操作系统语言设置为：", language)
if fan_advanced_effect[19] == 0:
    fan_advanced_effect[19] = [
     0, 255]
if PID_COMPAY == 8:
    if fan_advanced_effect[0] > 2:
        fan_advanced_effect[0] = 2
# global ARGB_FAN_config ## Warning: Unused global
# global ARGB_TEN ## Warning: Unused global
# global ARGB_TEN_MODEL1 ## Warning: Unused global
# global ARGB_dan_config ## Warning: Unused global
# global ARGB_dan_sync_config_chuji ## Warning: Unused global
# global ARGB_dan_sync_config_gaoji ## Warning: Unused global
# global ARGB_neiwang ## Warning: Unused global
# global ARGB_neiwang_exe ## Warning: Unused global
# global ARGB_neiwang_hex ## Warning: Unused global
# global ARGB_qun_async_config ## Warning: Unused global
# global FAN_speed ## Warning: Unused global
# global GONG_NEI ## Warning: Unused global
# global HEX_UPLOADER_ERROR ## Warning: Unused global
# global Light_mode ## Warning: Unused global
# global MCU_C_FLAG ## Warning: Unused global
# global ON ## Warning: Unused global
# global On_Effect ## Warning: Unused global
# global device_state ## Warning: Unused global
# global diannao_shiyin_yanse ## Warning: Unused global
# global fan_advanced_effect ## Warning: Unused global
# global fan_argb_button_model_index ## Warning: Unused global
# global fan_basic_effect ## Warning: Unused global
# global gaoji_ten_dengxiao_leixing ## Warning: Unused global
# global language ## Warning: Unused global
# global minimize ## Warning: Unused global
# global renwulan_index ## Warning: Unused global
# global tp ## Warning: Unused global
# ==== .write_argb_fan_all (line 2552)
with open(file_path_fan_config, encoding="utf-8") as file:
    data = json.load(file)
data["fan_ten"] = ARGB_TEN
data["fan_config"] = ARGB_FAN_config
data["fan_dan"] = ARGB_dan_config
data["fan_dan_sync_chuji"] = ARGB_dan_sync_config_chuji
data["fan_dan_sync_gaoji"] = ARGB_dan_sync_config_gaoji
data["fan_qun_async"] = ARGB_qun_async_config
with open(file_path_fan_config, "w", encoding="utf-8") as file:
    json.dump(data, file, ensure_ascii=False, indent=4)
# ==== .argb_jiyi_read (line 2584)
with open(file_argb_jiyi_config, encoding="utf-8") as file:
    data = json.load(file)
fan_argb_button_model_index = data["fan_argb_button_model_index"]
gaoji_ten_dengxiao_leixing = data["gaoji_ten_dengxiao_leixing"]
ARGB_TEN_MODEL1 = data["ARGB_TEN_MODEL1"]
fan_basic_effect = data["fan_basic_effect"]
fan_advanced_effect = data["fan_advanced_effect"]
MCU_C_FLAG = data["MCU_C_FLAG"]
FAN_speed = data["FAN_speed"]
HEX_UPLOADER_ERROR = data["HEX_UPLOADER_ERROR"]
GONG_NEI = data["GONG_NEI"]
On_Effect = data["On_Effect"]
return (
 fan_argb_button_model_index, gaoji_ten_dengxiao_leixing, ARGB_TEN_MODEL1,
 fan_basic_effect, fan_advanced_effect,
 MCU_C_FLAG, FAN_speed, HEX_UPLOADER_ERROR, GONG_NEI, On_Effect)
# ==== .argb_jiyi_write (line 2610)
print("保存配置文件")
with open(file_argb_jiyi_config, encoding="utf-8") as file:
    data = json.load(file)
if PID_COMPAY == 8:
    if not fan_argb_button_model_index == 0 or fan_argb_button_model_index == 1:
        pass
    else:
        data["fan_argb_button_model_index"] = fan_argb_button_model_index
elif fan_argb_button_model_index == 0:
    data["fan_argb_button_model_index"] = 1
else:
    data["fan_argb_button_model_index"] = fan_argb_button_model_index
data["gaoji_ten_dengxiao_leixing"] = gaoji_ten_dengxiao_leixing
data["ARGB_TEN_MODEL1"] = ARGB_TEN_MODEL1
data["fan_basic_effect"] = fan_basic_effect
data["fan_advanced_effect"] = fan_advanced_effect
data["MCU_C_FLAG"] = MCU_C_FLAG
data["FAN_speed"] = FAN_speed
data["HEX_UPLOADER_ERROR"] = HEX_UPLOADER_ERROR
data["GONG_NEI"] = GONG_NEI
data["On_Effect"] = On_Effect
with open(file_argb_jiyi_config, "w", encoding="utf-8") as file:
    json.dump(data, file, ensure_ascii=False, indent=4)
# ==== .argb_fan_num (line 2788)
jimu_num = 0
for i in ARGB_TEN_LEIXING:
    if i in jimu_lianpai:
        jimu_num += 1

if jimu_num >= 2:
    return True
return False
# ==== .argb_jimu_num (line 2800)
max_num = max(device_whide_ten)
jimu_num = max_num // 150
return jimu_num