
# ==== .hid_write (linha 417)
temp_z = len(list) // 64
temp_y = len(list) % 64
if temp_y != 0:
    temp_z = temp_z + 1
for i in range(temp_z):
    try:
        ser.write([0] + list[i * 64:64 * (i + 1)])
    except Exception as e:
        try:
            print(e)
        finally:
            e = None
            del e
# ==== .argb_fan_read_config (linha 429)
# descompilação falhou (ParserError: Parse error at or near `COLLECTION_START' instruction at offset 0_0
); desmontagem:
# # pydisasm version 6.1.7
# # Python bytecode 3.8.0 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       argb_fan_read_config
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  6
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        429
# # Constants:
# #    0: None
# #    1: 0
# #    2: 82
# #    3: 66
# #    4: 7
# #    5: 1
# #    6: 255
# #    7: 256
# #    8: 10
# #    9: 64
# #   10: 1000
# #   11: ('timeout_ms',)
# #   12: '获取10路设备信息：'
# #   13: 4
# #   14: 'RGB风扇 获取所有设备信息异常: '
# # Names:
# #    0: sum
# #    1: append
# #    2: range
# #    3: write
# #    4: print
# #    5: read
# #    6: Exception
# # Varnames:
# #	ser, send_hex, send_hex_sum, i, data, e
# # Positional arguments:
# #	ser
# # Local variables:
# #    1: send_hex
# #    2: send_hex_sum
# #    3: i
# #    4: data
# #    5: e
# 
# # Method Name:       argb_fan_read_config
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  6
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        429
# # Constants:
# #    0: None
# #    1: 0
# #    2: 82
# #    3: 66
# #    4: 7
# #    5: 1
# #    6: 255
# #    7: 256
# #    8: 10
# #    9: 64
# #   10: 1000
# #   11: ('timeout_ms',)
# #   12: '获取10路设备信息：'
# #   13: 4
# #   14: 'RGB风扇 获取所有设备信息异常: '
# # Names:
# #    0: sum
# #    1: append
# #    2: range
# #    3: write
# #    4: print
# #    5: read
# #    6: Exception
# # Varnames:
# #	ser, send_hex, send_hex_sum, i, data, e
# # Positional arguments:
# #	ser
# # Local variables:
# #    1: send_hex
# #    2: send_hex_sum
# #    3: i
# #    4: data
# #    5: e
# 430:           0 LOAD_CONST           (0)
#                2 LOAD_CONST           (82)
#                4 LOAD_CONST           (66)
#                6 LOAD_CONST           (7)
#                8 LOAD_CONST           (0)
#               10 LOAD_CONST           (1)
#               12 LOAD_CONST           (255)
#               14 BUILD_LIST           7
#               16 STORE_FAST           (send_hex)
# 
# 431:          18 LOAD_GLOBAL          (sum)
#               20 LOAD_FAST            (send_hex)
#               22 CALL_FUNCTION        (1 positional argument)
#               24 LOAD_CONST           (256)
#               26 BINARY_MODULO
#               28 STORE_FAST           (send_hex_sum)
# 
# 432:          30 LOAD_FAST            (send_hex)
#               32 LOAD_METHOD          (append)
#               34 LOAD_FAST            (send_hex_sum)
#               36 CALL_METHOD          (1 positional argument)
#               38 POP_TOP
# 
# 433:          40 SETUP_FINALLY        (to 138)
# 
# 434:          42 LOAD_GLOBAL          (range)
#               44 LOAD_CONST           (10)
#               46 CALL_FUNCTION        (1 positional argument)
#               48 GET_ITER
#          >>   50 FOR_ITER             (to 132)
#               52 STORE_FAST           (i)
# 
# 435:          54 LOAD_FAST            (ser)
#               56 LOAD_METHOD          (write)
#               58 LOAD_FAST            (send_hex)
#               60 CALL_METHOD          (1 positional argument)
#               62 POP_TOP
# 
# 436:          64 LOAD_GLOBAL          (print)
#               66 LOAD_FAST            (send_hex)
#               68 CALL_FUNCTION        (1 positional argument)
#               70 POP_TOP
# 
# 437:          72 LOAD_FAST            (ser)
#               74 LOAD_ATTR            (read)
#               76 LOAD_CONST           (64)
#               78 LOAD_CONST           (1000)
#               80 LOAD_CONST           (('timeout_ms',))
#               82 CALL_FUNCTION_KW     (2 total positional and keyword args)
#               84 STORE_FAST           (data)
# 
# 438:          86 LOAD_GLOBAL          (print)
#               88 LOAD_CONST           ("获取10路设备信息：")
#               90 LOAD_FAST            (data)
#               92 FORMAT_VALUE         0
#               94 BUILD_STRING         2
#               96 CALL_FUNCTION        (1 positional argument)
#               98 POP_TOP
# 
# 439:         100 LOAD_FAST            (data)
#              102 BUILD_LIST           0
#              104 COMPARE_OP           (!=)
#              106 POP_JUMP_IF_FALSE    (to 50)
# 
# 440:         108 LOAD_FAST            (data)
#              110 LOAD_CONST           (4)
#              112 BINARY_SUBSCR
#              114 LOAD_CONST           (1)
#              116 COMPARE_OP           (==)
#              118 POP_JUMP_IF_FALSE    (to 50)
# 
# 441:         120 LOAD_FAST            (data)
#              122 ROT_TWO
#              124 POP_TOP
#              126 POP_BLOCK
#              128 RETURN_VALUE
#              130 JUMP_ABSOLUTE        (to 50)
# 
# 442:     >>  132 POP_BLOCK
#              134 LOAD_CONST           (None)
#              136 RETURN_VALUE
# 
# 443:     >>  138 DUP_TOP
#              140 LOAD_GLOBAL          (Exception)
#              142 COMPARE_OP           (exception-match)
#              144 POP_JUMP_IF_FALSE    (to 190)
#              146 POP_TOP
#              148 STORE_FAST           (e)
#              150 POP_TOP
#              152 SETUP_FINALLY        (to 178)
# 
# 444:         154 LOAD_GLOBAL          (print)
#              156 LOAD_CONST           ("RGB风扇 获取所有设备信息异常: ")
#              158 LOAD_FAST            (e)
#              160 FORMAT_VALUE         0
#              162 BUILD_STRING         2
#              164 CALL_FUNCTION        (1 positional argument)
#              166 POP_TOP
# 
# 445:         168 POP_BLOCK
#              170 POP_EXCEPT
#              172 CALL_FINALLY         (to 178)
#              174 LOAD_CONST           (None)
#              176 RETURN_VALUE
#          >>  178 LOAD_CONST           (None)
#              180 STORE_FAST           (e)
#              182 DELETE_FAST          (e)
#              184 END_FINALLY
#              186 POP_EXCEPT
#              188 JUMP_FORWARD         (to 192)
#          >>  190 END_FINALLY
#          >>  192 LOAD_CONST           (None)
#              194 RETURN_VALUE
# 
# 
# ==== .MainWindow.table1_UI (linha 20651)
rgb_rgb_layout = QHBoxLayout()
rgb_r_layout = QHBoxLayout()
rgb_g_layout = QHBoxLayout()
rgb_b_layout = QHBoxLayout()
self.rgb_r_lable = QLabel("红")
self.rgb_r_lable.setFont(self.font)
self.rgb_r_lable.setStyleSheet("color: white; font-size: 16px;")
self.radio_button10 = QRadioButton("R")
self.radio_button10.setFont(self.font)
self.radio_button10.setStyleSheet("color: white; font-size: 16px;")
self.radio_button11 = QRadioButton("G")
self.radio_button11.setFont(self.font)
self.radio_button11.setStyleSheet("color: white; font-size: 16px;")
self.radio_button12 = QRadioButton("B")
self.radio_button12.setFont(self.font)
self.radio_button12.setStyleSheet("color: white; font-size: 16px;")
self.radio_button10.setProperty("id", "RR")
self.radio_button11.setProperty("id", "RG")
self.radio_button12.setProperty("id", "RB")
rgb_r_layout.addWidget(self.rgb_r_lable)
rgb_r_layout.addWidget(self.radio_button10)
rgb_r_layout.addWidget(self.radio_button11)
rgb_r_layout.addWidget(self.radio_button12)
rgb_r_layout.addStretch(1)
self.rgb_g_lable = QLabel("绿")
self.rgb_g_lable.setFont(self.font)
self.rgb_g_lable.setStyleSheet("color: white; font-size: 16px;")
self.radio_button13 = QRadioButton("R")
self.radio_button13.setFont(self.font)
self.radio_button13.setStyleSheet("color: white; font-size: 16px;")
self.radio_button14 = QRadioButton("G")
self.radio_button14.setFont(self.font)
self.radio_button14.setStyleSheet("color: white; font-size: 16px;")
self.radio_button15 = QRadioButton("B")
self.radio_button15.setFont(self.font)
self.radio_button15.setStyleSheet("color: white; font-size: 16px;")
self.radio_button13.setProperty("id", "GR")
self.radio_button14.setProperty("id", "GG")
self.radio_button15.setProperty("id", "GB")
rgb_g_layout.addWidget(self.rgb_g_lable)
rgb_g_layout.addWidget(self.radio_button13)
rgb_g_layout.addWidget(self.radio_button14)
rgb_g_layout.addWidget(self.radio_button15)
rgb_g_layout.addStretch(1)
self.rgb_b_lable = QLabel("蓝")
self.rgb_b_lable.setFont(self.font)
self.rgb_b_lable.setStyleSheet("color: white; font-size: 16px;")
self.radio_button16 = QRadioButton("R")
self.radio_button16.setFont(self.font)
self.radio_button16.setStyleSheet("color: white; font-size: 16px;")
self.radio_button17 = QRadioButton("G")
self.radio_button17.setFont(self.font)
self.radio_button17.setStyleSheet("color: white; font-size: 16px;")
self.radio_button18 = QRadioButton("B")
self.radio_button18.setFont(self.font)
self.radio_button18.setStyleSheet("color: white; font-size: 16px;")
self.radio_button16.setProperty("id", "BR")
self.radio_button17.setProperty("id", "BG")
self.radio_button18.setProperty("id", "BB")
rgb_b_layout.addWidget(self.rgb_b_lable)
rgb_b_layout.addWidget(self.radio_button16)
rgb_b_layout.addWidget(self.radio_button17)
rgb_b_layout.addWidget(self.radio_button18)
rgb_b_layout.addStretch(1)
rgb_rgb_layout.addLayout(rgb_r_layout)
rgb_rgb_layout.addLayout(rgb_g_layout)
rgb_rgb_layout.addLayout(rgb_b_layout)
self.table1_layout.addLayout(rgb_rgb_layout)
# ==== .MainWindow.table2_UI (linha 20729)
h_layout = QVBoxLayout()
rgb_r_layout = QHBoxLayout()
rgb_g_layout = QHBoxLayout()
self.radio_button19 = QRadioButton("显示器背部")
self.radio_button19.setFont(self.font)
self.radio_button19.setStyleSheet("color: white; font-size: 16px;")
self.radio_button20 = QRadioButton("显示器上侧")
self.radio_button20.setFont(self.font)
self.radio_button20.setStyleSheet("color: white; font-size: 16px;")
self.radio_button21 = QRadioButton("显示器下侧")
self.radio_button21.setFont(self.font)
self.radio_button21.setStyleSheet("color: white; font-size: 16px;")
self.radio_button39 = QRadioButton("显示器平均色")
self.radio_button39.setFont(self.font)
self.radio_button39.setStyleSheet("color: white; font-size: 16px;")
self.radio_button22 = QRadioButton("显示器左侧")
self.radio_button22.setFont(self.font)
self.radio_button22.setStyleSheet("color: white; font-size: 16px;")
self.radio_button23 = QRadioButton("显示器右侧")
self.radio_button23.setFont(self.font)
self.radio_button23.setStyleSheet("color: white; font-size: 16px;")
self.radio_button24 = QRadioButton("显示器左右两侧")
self.radio_button24.setFont(self.font)
self.radio_button24.setStyleSheet("color: white; font-size: 16px;")
self.radio_button19.setProperty("id", "00")
self.radio_button20.setProperty("id", "01")
self.radio_button21.setProperty("id", "02")
self.radio_button22.setProperty("id", "03")
self.radio_button23.setProperty("id", "04")
self.radio_button24.setProperty("id", "06")
self.radio_button39.setProperty("id", "05")
rgb_r_layout.addWidget(self.radio_button19)
rgb_r_layout.addWidget(self.radio_button20)
rgb_r_layout.addWidget(self.radio_button21)
rgb_r_layout.addWidget(self.radio_button39)
rgb_r_layout.addStretch(1)
rgb_g_layout.addWidget(self.radio_button22)
rgb_g_layout.addWidget(self.radio_button23)
rgb_g_layout.addWidget(self.radio_button24)
rgb_g_layout.addStretch(1)
h_layout.addLayout(rgb_r_layout)
h_layout.addLayout(rgb_g_layout)
self.table2_layout.addLayout(h_layout)
# ==== .MainWindow.table3_UI (linha 20778)
self.bianshu_layout = QHBoxLayout()
self.radio_button25 = QRadioButton("三边")
self.radio_button25.setFont(self.font)
self.radio_button25.setStyleSheet("color: white; font-size: 16px;")
self.radio_button26 = QRadioButton("四边")
self.radio_button26.setFont(self.font)
self.radio_button26.setStyleSheet("color: white; font-size: 16px;")
self.radio_button25.setProperty("id", 3)
self.radio_button26.setProperty("id", 4)
self.bianshu_layout.addWidget(self.radio_button25)
self.bianshu_layout.addWidget(self.radio_button26)
self.bianshu_layout.addStretch(1)
self.table3_layout.addLayout(self.bianshu_layout)
# ==== .MainWindow.table4_UI (linha 20795)
self.rgb_1m_number_layout = QHBoxLayout()
self.radio_button27 = QRadioButton("5V/1M/60")
self.radio_button27.setFont(self.font)
self.radio_button27.setStyleSheet("color: white; font-size: 16px;")
self.radio_button28 = QRadioButton("5V/1M/30")
self.radio_button28.setFont(self.font)
self.radio_button28.setStyleSheet("color: white; font-size: 16px;")
self.radio_button29 = QRadioButton("12V/1M/60")
self.radio_button29.setFont(self.font)
self.radio_button29.setStyleSheet("color: white; font-size: 16px;")
self.radio_button38 = QRadioButton("24V/1M/84")
self.radio_button38.setFont(self.font)
self.radio_button38.setStyleSheet("color: white; font-size: 16px;")
self.radio_button27.setProperty("id", 60)
self.radio_button28.setProperty("id", 30)
self.radio_button29.setProperty("id", 20)
self.radio_button38.setProperty("id", 14)
self.rgb_1m_number_layout.addWidget(self.radio_button27)
self.rgb_1m_number_layout.addWidget(self.radio_button28)
self.rgb_1m_number_layout.addWidget(self.radio_button29)
self.rgb_1m_number_layout.addWidget(self.radio_button38)
self.rgb_1m_number_layout.addStretch(1)
self.table4_layout.addLayout(self.rgb_1m_number_layout)
# ==== .MainWindow.table5_UI (linha 20821)
anzhuang_layout = QHBoxLayout()
self.radio_button30 = QRadioButton("从左到右")
self.radio_button30.setFont(self.font)
self.radio_button30.setStyleSheet("color: white; font-size: 16px;")
self.radio_button31 = QRadioButton("从右到左")
self.radio_button31.setFont(self.font)
self.radio_button31.setStyleSheet("color: white; font-size: 16px;")
self.radio_button30.setProperty("id", "left")
self.radio_button31.setProperty("id", "right")
anzhuang_layout.addWidget(self.radio_button30)
anzhuang_layout.addWidget(self.radio_button31)
anzhuang_layout.addStretch(1)
self.table5_layout.addLayout(anzhuang_layout)
# ==== .MainWindow.table6_UI (linha 20837)
shebei_layout = QHBoxLayout()
self.text_label3 = QLabel("调整灯珠数量")
self.text_label3.setFont(self.font)
self.text_label3.setStyleSheet("color: white; font-size: 16px; ")
self.button_add = QPushButton()
self.button_add.setIcon(QIcon(port.file_path_img("img/+.svg")))
self.button_add.setIconSize(QtCore.QSize(40, 40))
self.button_add.setStyleSheet("border-radius: 16px;")
self.button_add.clicked.connect(self.add_light_num)
self.slider1 = QSlider(Qt.Horizontal)
self.slider1.setFocusPolicy(Qt.NoFocus)
self.slider1.setRange(1, 254)
self.slider1.setStyleSheet("\n                                                                            QSlider::groove:horizontal {\n                                                                                background-color: #f3f4f5;\n                                                                                height: 16px;\n                                                                                border-radius: 10px;\n                                                                            }\n\n                                                                            QSlider::sub-page:horizontal {\n                                                                                background-color: #114eff;\n                                                                                height: 16px;\n                                                                                border-radius: 10px;\n                                                                            }\n\n                                                                            QSlider::handle:horizontal {\n                                                                                background-color: #114eff;\n                                                                                # border: 1px solid #FFF;\n                                                                                width: 16px;\n                                                                                height: 16px;\n                                                                                margin: -1px 0;\n                                                                                border-radius: 10px;\n                                                                            }\n                                                                        ")
self.slider1.valueChanged.connect(self.changer_light_num)
self.slider1.mousePressEvent = self.slider2_mousePressEvent
self.button_reduce = QPushButton()
self.button_reduce.setIcon(QIcon(port.file_path_img("img/-.svg")))
self.button_reduce.setIconSize(QtCore.QSize(40, 40))
self.button_reduce.setStyleSheet("border-radius: 16px;")
self.button_reduce.clicked.connect(self.reduce_light_num)
self.text_input = QLabel("254")
self.text_input.setFont(self.font)
self.text_input.setStyleSheet("color: white; font-size: 16px; ")
shebei_layout.addWidget(self.button_reduce)
shebei_layout.addWidget(self.slider1)
shebei_layout.addWidget(self.button_add)
shebei_layout.addWidget(self.text_input)
self.table6_layout.addLayout(shebei_layout)
# ==== .MainWindow.table7_UI (linha 20896)
onoff_layout = QHBoxLayout()
onoff_on_layout = QHBoxLayout()
onoff_min_layout = QHBoxLayout()
self.off_lable = QLabel("自动关灯")
self.off_lable.setFont(self.font)
self.off_lable.setStyleSheet("color: white; font-size: 16px;")
self.radio_button32 = QRadioButton("开启")
self.radio_button32.setFont(self.font)
self.radio_button32.setStyleSheet("color: white; font-size: 16px;")
self.radio_button33 = QRadioButton("关闭")
self.radio_button33.setFont(self.font)
self.radio_button33.setStyleSheet("color: white; font-size: 16px;")
self.radio_button32.setProperty("id", "off_on")
self.radio_button33.setProperty("id", "off_off")
onoff_on_layout.addWidget(self.radio_button32)
onoff_on_layout.addWidget(self.radio_button33)
onoff_on_layout.addStretch(1)
self.off_time_lable = QLabel("选择时长")
self.off_time_lable.setFont(self.font)
self.off_time_lable.setStyleSheet("color: white; font-size: 16px;")
self.radio_button34 = QRadioButton("5分钟")
self.radio_button34.setFont(self.font)
self.radio_button34.setStyleSheet("color: white; font-size: 16px;")
self.radio_button35 = QRadioButton("10分钟")
self.radio_button35.setFont(self.font)
self.radio_button35.setStyleSheet("color: white; font-size: 16px;")
self.radio_button36 = QRadioButton("30分钟")
self.radio_button36.setFont(self.font)
self.radio_button36.setStyleSheet("color: white; font-size: 16px;")
self.radio_button37 = QRadioButton("60分钟")
self.radio_button37.setFont(self.font)
self.radio_button37.setStyleSheet("color: white; font-size: 16px;")
self.radio_button34.setProperty("id", "time5")
self.radio_button35.setProperty("id", "time10")
self.radio_button36.setProperty("id", "time30")
self.radio_button37.setProperty("id", "time60")
onoff_min_layout.addWidget(self.off_time_lable)
onoff_min_layout.addWidget(self.radio_button34)
onoff_min_layout.addWidget(self.radio_button35)
onoff_min_layout.addWidget(self.radio_button36)
onoff_min_layout.addWidget(self.radio_button37)
onoff_min_layout.addStretch(1)
onoff_layout.addLayout(onoff_on_layout)
onoff_layout.addLayout(onoff_min_layout)
self.table7_layout.addLayout(onoff_layout)
# ==== .MainWindow.dan_tiaose (linha 21068)
self.tiaose_index += 1
color_dialog = QColorDialog(self)
color_dialog.setOption(QColorDialog.ColorDialogOption.DontUseNativeDialog)
color = color_dialog.getColor()
if color.isValid():
    print("选择颜色:", color.name())
    tiaose = test.hex_to_rgb(color.name())
    self.button_tiaose.setStyleSheet(f"background-color: rgb{tuple(tiaose)}; color: white; border-radius: 15px; font-size: 16px;")
    self.slider_huancai_speed.hide()
    self.slider_shiyin_speed.hide()
    self.button_onoff.setIcon(QIcon(port.file_path_img("img/light-on.svg")))
    self.button_onoff.setCheckable(True)
    self.button_tongping.setIcon(QIcon(port.file_path_img("img/fantasy1.svg")))
    self.button_tongping.setCheckable(True)
    self.sender_color = self.sender()
    try:
        if self.devices_qun_button == 1:
            test.quse_exit_event_dongle.set()
            test.tongping_event_list = []
            port.devices_qun[2] = self.tiaose_index
            port.devices_qun[0] = 1
            port.devices_qun[4] = 0
            port.devices_qun[5] = 0
            port.devices_qun[6] = 0
            port.devices_qun_color = tiaose
            for i in range(len(port.usb_device_sn)):
                port.devices_ten[i][2] = self.tiaose_index
                port.devices_ten[i][0] = 1
                port.devices_ten[i][4] = 0
                port.devices_ten[i][5] = 0
                port.devices_ten[i][6] = 0
                port.devices_ten_color[i] = tiaose
                test.device_dan_events[i].set()

            try:

                def danxuanyanse_qun():
                    time.sleep(0.1)
                    if "dongle" in port.device_info_robobloq:
                        test.NO_tongping(port.ser_list[port.device_info_robobloq.index("dongle")], [
                         255,255,255,255,255,255] + [3, port.devices_qun_color[0],
                         port.devices_qun_color[1],
                         port.devices_qun_color[2], 3, port.devices_qun_color[0],
                         port.devices_qun_color[1], port.devices_qun_color[2]])
                        test.tongping_none = [255,255,255,255,255,255] + [3, port.devices_qun_color[0],
                         port.devices_qun_color[1],
                         port.devices_qun_color[2], 3,
                         port.devices_qun_color[0],
                         port.devices_qun_color[1],
                         port.devices_qun_color[2]]
                    for i in range(len(port.usb_device_sn)):
                        if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
                            test.NO_tongping(port.ser_list[i], [
                             255,255,255,255,255,255] + [3, port.devices_qun_color[0],
                             port.devices_qun_color[1],
                             port.devices_qun_color[2], 3,
                             port.devices_qun_color[0],
                             port.devices_qun_color[1],
                             port.devices_qun_color[2]])
                            test.device_dan_no_tongping[i] = [3, port.devices_qun_color[0],
                             port.devices_qun_color[1],
                             port.devices_qun_color[2]]
                        if port.device_info_robobloq[i] == "usb_hid":
                            agreement.usb_hid_monochrome(port.devices_qun_color, port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])


                t = threading.Thread(target=danxuanyanse_qun, daemon=True)
                t.start()
            except:
                pass

        button_index = self.device_dan_button - 1
        port.devices_ten_color[self.device_dan_button - 1] = test.ya_rgb_one(tiaose)
        port.devices_ten[self.device_dan_button - 1][2] = self.tiaose_index
        port.devices_ten[self.device_dan_button - 1][0] = 1
        port.devices_ten[self.device_dan_button - 1][4] = 0
        port.devices_ten[self.device_dan_button - 1][5] = 0
        port.devices_ten[self.device_dan_button - 1][6] = 0
        test.device_dan_events[self.device_dan_button - 1].set()
        self.panduan_tongping()
        try:

            def danxuanyanse_dan(colors_array, but_index):
                time.sleep(0.1)
                index = 0
                if port.device_info_robobloq[but_index] == "dongle":
                    if test.thread_alive("dongle"):
                        test.tongping_none = port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
                         colors_array[index][1],
                         colors_array[index][2]]
                    else:
                        test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
                         colors_array[index][1],
                         colors_array[index][2]])
                if port.device_info_robobloq[but_index] == "usb" or port.device_info_robobloq[but_index] == "sound":
                    test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[but_index][0:6] + [3, colors_array[index][0],
                     colors_array[index][1],
                     colors_array[index][2]])
                    test.device_dan_no_tongping[but_index] = [but_index][0:6] + [3, colors_array[index][0],
                     colors_array[index][1],
                     colors_array[index][2]]
                if port.device_info_robobloq[but_index] == "usb_hid":
                    agreement.usb_hid_monochrome(port.devices_ten_color[but_index], port.ser_list[but_index], port.usb_device_rgbnum[but_index], port.usb_device_RGB[but_index])


            colors_array_list = [tiaose]
            t = threading.Thread(target=(lambda: danxuanyanse_dan(colors_array_list, button_index)), daemon=True)
            t.start()
        except:
            pass

    except Exception as e:
        try:
            print("发生错误:", e)
        finally:
            e = None
            del e

    else:
        for i, btn in enumerate(self.shiyin_buttons):
            btn.setChecked(False)
            self.shiyin_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.shiyin_img[i]))))

        for i, btn in enumerate(self.huancai_buttons):
            btn.setChecked(False)
            self.huancai_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.huancai_img[i]))))

else:
    print("取消选择颜色")
# ==== .MainWindow.dan_tiaose.danxuanyanse_qun (linha 21105)
time.sleep(0.1)
if "dongle" in port.device_info_robobloq:
    test.NO_tongping(port.ser_list[port.device_info_robobloq.index("dongle")], [
     255,255,255,255,255,255] + [3, port.devices_qun_color[0],
     port.devices_qun_color[1],
     port.devices_qun_color[2], 3, port.devices_qun_color[0],
     port.devices_qun_color[1], port.devices_qun_color[2]])
    test.tongping_none = [255,255,255,255,255,255] + [3, port.devices_qun_color[0],
     port.devices_qun_color[1],
     port.devices_qun_color[2], 3,
     port.devices_qun_color[0],
     port.devices_qun_color[1],
     port.devices_qun_color[2]]
for i in range(len(port.usb_device_sn)):
    if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
        test.NO_tongping(port.ser_list[i], [
         255,255,255,255,255,255] + [3, port.devices_qun_color[0],
         port.devices_qun_color[1],
         port.devices_qun_color[2], 3,
         port.devices_qun_color[0],
         port.devices_qun_color[1],
         port.devices_qun_color[2]])
        test.device_dan_no_tongping[i] = [3, port.devices_qun_color[0],
         port.devices_qun_color[1],
         port.devices_qun_color[2]]
    if port.device_info_robobloq[i] == "usb_hid":
        agreement.usb_hid_monochrome(port.devices_qun_color, port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])
# ==== .MainWindow.dan_tiaose.danxuanyanse_dan (linha 21151)
time.sleep(0.1)
index = 0
if port.device_info_robobloq[but_index] == "dongle":
    if test.thread_alive("dongle"):
        test.tongping_none = port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
         colors_array[index][1],
         colors_array[index][2]]
    else:
        test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
         colors_array[index][1],
         colors_array[index][2]])
if port.device_info_robobloq[but_index] == "usb" or port.device_info_robobloq[but_index] == "sound":
    test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[but_index][0:6] + [3, colors_array[index][0],
     colors_array[index][1],
     colors_array[index][2]])
    test.device_dan_no_tongping[but_index] = [but_index][0:6] + [3, colors_array[index][0],
     colors_array[index][1],
     colors_array[index][2]]
if port.device_info_robobloq[but_index] == "usb_hid":
    agreement.usb_hid_monochrome(port.devices_ten_color[but_index], port.ser_list[but_index], port.usb_device_rgbnum[but_index], port.usb_device_RGB[but_index])
# ==== .MainWindow.buttonClicked (linha 21198)
if len(port.usb_device_sn) == 0:
    self.tanchuang_no_device()
else:
    self.slider_huancai_speed.hide()
    self.slider_shiyin_speed.hide()
    self.button_onoff.setIcon(QIcon(port.file_path_img("img/light-on.svg")))
    self.button_onoff.setCheckable(True)
    self.button_tongping.setIcon(QIcon(port.file_path_img("img/fantasy1.svg")))
    self.button_tongping.setCheckable(True)
    self.sender_color = self.sender()
    try:
        if self.sender_color.isChecked():
            index = self.buttons.index(self.sender_color)
            colors_array = np.array(self.colors).tolist()
            if self.devices_qun_button == 1:
                test.quse_exit_event_dongle.set()
                test.tongping_event_list = []
                port.devices_qun[2] = index + 1
                port.devices_qun[0] = 1
                port.devices_qun[4] = 0
                port.devices_qun[5] = 0
                port.devices_qun[6] = 0
                port.devices_qun_color = test.ya_rgb_one(colors_array[index])
                for i in range(len(port.usb_device_sn)):
                    port.devices_ten[i][2] = index + 1
                    port.devices_ten[i][0] = 1
                    port.devices_ten[i][4] = 0
                    port.devices_ten[i][5] = 0
                    port.devices_ten[i][6] = 0
                    port.devices_ten_color[i] = colors_array[index]
                    test.device_dan_events[i].set()

                absolute_path = port.file_path_img("img/duihao.png")
                url = ""
                for i in absolute_path:
                    if i == "\\":
                        url = url + "/"
                    else:
                        url = url + i

                self.sender_color.setStyleSheet(f"QPushButton {{ background-image: url({url}); background-repeat: no-repeat; background-position: center; background-color: rgb{self.colors[index]}; border-radius: 16px; }}")
                print(f"Button {index} is checked")
                for button in self.buttons:
                    if button is not self.sender_color:
                        button.setChecked(False)
                    try:

                        def danxuanyanse_qun():
                            time.sleep(0.1)
                            if "dongle" in port.device_info_robobloq:
                                test.NO_tongping(port.ser_list[port.device_info_robobloq.index("dongle")], [
                                 255,255,255,255,255,255] + [3, port.devices_qun_color[0],
                                 port.devices_qun_color[1],
                                 port.devices_qun_color[2], 3, port.devices_qun_color[0],
                                 port.devices_qun_color[1], port.devices_qun_color[2]])
                                test.tongping_none = [255,255,255,255,255,255] + [3, port.devices_qun_color[0],
                                 port.devices_qun_color[1],
                                 port.devices_qun_color[2], 3,
                                 port.devices_qun_color[0],
                                 port.devices_qun_color[1],
                                 port.devices_qun_color[2]]
                            for i in range(len(port.usb_device_sn)):
                                if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
                                    test.NO_tongping(port.ser_list[i], [
                                     255,255,255,255,255,255] + [3, port.devices_qun_color[0],
                                     port.devices_qun_color[1],
                                     port.devices_qun_color[2], 3,
                                     port.devices_qun_color[0],
                                     port.devices_qun_color[1],
                                     port.devices_qun_color[2]])
                                    test.device_dan_no_tongping[i] = [3, port.devices_qun_color[0],
                                     port.devices_qun_color[1],
                                     port.devices_qun_color[2]]
                                if port.device_info_robobloq[i] == "usb_hid":
                                    agreement.usb_hid_monochrome(port.devices_qun_color, port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])


                        t = threading.Thread(target=danxuanyanse_qun, daemon=True)
                        t.start()
                    except:
                        pass

                button_index = self.device_dan_button - 1
                port.devices_ten_color[self.device_dan_button - 1] = test.ya_rgb_one(colors_array[index])
                port.devices_ten[self.device_dan_button - 1][2] = index + 1
                port.devices_ten[self.device_dan_button - 1][0] = 1
                port.devices_ten[self.device_dan_button - 1][4] = 0
                port.devices_ten[self.device_dan_button - 1][5] = 0
                port.devices_ten[self.device_dan_button - 1][6] = 0
                test.device_dan_events[self.device_dan_button - 1].set()
                self.panduan_tongping()
                print("单设备更改颜色", port.devices_ten_color)
                absolute_path = port.file_path_img("img/duihao.png")
                url = ""
                for i in absolute_path:
                    if i == "\\":
                        url = url + "/"
                    else:
                        url = url + i

                self.sender_color.setStyleSheet(f"QPushButton {{ background-image: url({url}); background-repeat: no-repeat; background-position: center; background-color: rgb{self.colors[index]}; border-radius: 16px; }}")
                print(f"Button {index} is checked")
                for button in self.buttons:
                    if button is not self.sender_color:
                        button.setChecked(False)

            try:

                def danxuanyanse_dan(colors_array, but_index):
                    time.sleep(0.1)
                    if port.device_info_robobloq[but_index] == "dongle":
                        if test.thread_alive("dongle"):
                            test.tongping_none = port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
                             colors_array[index][1],
                             colors_array[index][2],
                             255,
                             255,
                             255,
                             255]
                        else:
                            test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
                             colors_array[index][1],
                             colors_array[index][2],
                             255,
                             255,
                             255,
                             255])
                    if port.device_info_robobloq[but_index] == "usb" or port.device_info_robobloq[but_index] == "sound":
                        test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[but_index][0:6] + [3, colors_array[index][0],
                         colors_array[index][1],
                         colors_array[index][2],
                         255,
                         255,
                         255,
                         255])
                        test.device_dan_no_tongping[but_index] = [but_index][0:6] + [3, colors_array[index][0],
                         colors_array[index][1],
                         colors_array[index][2]]
                    if port.device_info_robobloq[but_index] == "usb_hid":
                        agreement.usb_hid_monochrome(port.devices_ten_color[but_index], port.ser_list[but_index], port.usb_device_rgbnum[but_index], port.usb_device_RGB[but_index])


                colors_array_list = colors_array
                t = threading.Thread(target=(lambda: danxuanyanse_dan(colors_array_list, button_index)), daemon=True)
                t.start()
            except:
                pass
            else:
                for i, btn in enumerate(self.buttons):
                    if i != index:
                        btn.setStyleSheet(f"background-color: rgb{self.colors[i]}; border-radius: 16px;")

    except Exception as e:
        try:
            print("发生错误:", e)
        finally:
            e = None
            del e

    else:
        for i, btn in enumerate(self.shiyin_buttons):
            btn.setChecked(False)
            self.shiyin_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.shiyin_img[i]))))

        for i, btn in enumerate(self.huancai_buttons):
            btn.setChecked(False)
            self.huancai_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.huancai_img[i]))))

# global t ## Warning: Unused global
# ==== .MainWindow.buttonClicked.danxuanyanse_qun (linha 21247)
time.sleep(0.1)
if "dongle" in port.device_info_robobloq:
    test.NO_tongping(port.ser_list[port.device_info_robobloq.index("dongle")], [
     255,255,255,255,255,255] + [3, port.devices_qun_color[0],
     port.devices_qun_color[1],
     port.devices_qun_color[2], 3, port.devices_qun_color[0],
     port.devices_qun_color[1], port.devices_qun_color[2]])
    test.tongping_none = [255,255,255,255,255,255] + [3, port.devices_qun_color[0],
     port.devices_qun_color[1],
     port.devices_qun_color[2], 3,
     port.devices_qun_color[0],
     port.devices_qun_color[1],
     port.devices_qun_color[2]]
for i in range(len(port.usb_device_sn)):
    if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
        test.NO_tongping(port.ser_list[i], [
         255,255,255,255,255,255] + [3, port.devices_qun_color[0],
         port.devices_qun_color[1],
         port.devices_qun_color[2], 3,
         port.devices_qun_color[0],
         port.devices_qun_color[1],
         port.devices_qun_color[2]])
        test.device_dan_no_tongping[i] = [3, port.devices_qun_color[0],
         port.devices_qun_color[1],
         port.devices_qun_color[2]]
    if port.device_info_robobloq[i] == "usb_hid":
        agreement.usb_hid_monochrome(port.devices_qun_color, port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])
# ==== .MainWindow.buttonClicked.danxuanyanse_dan (linha 21308)
time.sleep(0.1)
if port.device_info_robobloq[but_index] == "dongle":
    if test.thread_alive("dongle"):
        test.tongping_none = port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
         colors_array[index][1],
         colors_array[index][2],
         255,
         255,
         255,
         255]
    else:
        test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[self.device_dan_button - 1][0:6] + [3, colors_array[index][0],
         colors_array[index][1],
         colors_array[index][2],
         255,
         255,
         255,
         255])
if port.device_info_robobloq[but_index] == "usb" or port.device_info_robobloq[but_index] == "sound":
    test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[but_index][0:6] + [3, colors_array[index][0],
     colors_array[index][1],
     colors_array[index][2],
     255,
     255,
     255,
     255])
    test.device_dan_no_tongping[but_index] = [but_index][0:6] + [3, colors_array[index][0],
     colors_array[index][1],
     colors_array[index][2]]
if port.device_info_robobloq[but_index] == "usb_hid":
    agreement.usb_hid_monochrome(port.devices_ten_color[but_index], port.ser_list[but_index], port.usb_device_rgbnum[but_index], port.usb_device_RGB[but_index])
# ==== .MainWindow.fan_colors_buttonClicked (linha 21366)
self.fan_sender_color = self.sender()
if self.fan_sender_color.isChecked():
    effect = self.fan_effect_list.currentIndex()
    index = self.fan_dengxiao_buttons.index(self.fan_sender_color)
    self.fan_sender_color.setStyleSheet(f"QPushButton {{ background-image: url(); background-repeat: no-repeat; background-position: center; background-color: rgb{tuple(self.fan_tiaose_color)}; border-radius: 20px; }}")
    print(f"Button {index} is checked")
    colors_four_list = [[0, 0, 0] for _ in range(4)]
    if port.fan_advanced_effect[0] == 4 and port.On_Effect[0] == 1:
        agreement.fan_order_list = [0 for i in range(52)]
        port.On_Effect[5] = self.fan_tiaose_color[0]
        port.On_Effect[6] = self.fan_tiaose_color[1]
        port.On_Effect[7] = self.fan_tiaose_color[2]
        agreement.fan_order_list[0:16] = port.On_Effect[1:17]
        agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
        for i in range(4):
            colors_four_list[i][0] = port.On_Effect[5 + i * 3]
            colors_four_list[i][1] = port.On_Effect[5 + i * 3 + 1]
            colors_four_list[i][2] = port.On_Effect[5 + i * 3 + 2]

        port.write_argb_fan_all()
        port.argb_jiyi_write()
        for button in self.fan_dengxiao_buttons:
            if button is not self.fan_sender_color:
                button.setChecked(False)

        for i, btn in enumerate(self.fan_dengxiao_buttons):
            try:
                if i != index:
                    btn.setStyleSheet(f"background-color: rgb{tuple(colors_four_list[i])}; border-radius: 20px;")
            except:
                pass
# ==== .MainWindow.button_huancai_Clicked (linha 21469)
if len(port.usb_device_sn) == 0:
    self.tanchuang_no_device()
else:
    self.slider_huancai_speed.show()
    self.slider_shiyin_speed.hide()
    self.button_onoff.setIcon(QIcon(port.file_path_img("img/light-on.svg")))
    self.button_onoff.setCheckable(True)
    self.button_tongping.setIcon(QIcon(port.file_path_img("img/fantasy1.svg")))
    self.button_tongping.setCheckable(True)
    self.huancai_sender = self.sender()
    if self.huancai_sender.isChecked():
        index = self.huancai_buttons.index(self.huancai_sender)
        print(f"Button {index} is checked")
        for button in self.huancai_buttons:
            if button is not self.huancai_sender:
                button.setChecked(False)
            else:
                self.huancai_buttons[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.huancai_img_xuanzhong[index]))))

        for i, btn in enumerate(self.huancai_buttons):
            if i != index:
                self.huancai_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.huancai_img[i]))))

        try:
            if self.devices_qun_button == 1:
                test.quse_exit_event_dongle.set()
                test.tongping_event_list = []
                port.devices_qun[5] = index + 1
                port.devices_qun[2] = 0
                port.devices_qun[4] = 0
                port.devices_qun[6] = 0
                port.devices_qun[0] = 1
                for i in range(len(port.usb_device_sn)):
                    port.devices_ten[i][5] = index + 1
                    port.devices_ten[i][2] = 0
                    port.devices_ten[i][4] = 0
                    port.devices_ten[i][6] = 0
                    port.devices_ten[i][0] = 1
                    test.device_dan_events[i].set()

                def huancai_qun(index):
                    time.sleep(0.1)
                    dongle_index = agreement.find_first_index(port.device_info_robobloq, "dongle")
                    if "dongle" in port.device_info_robobloq:
                        test.NO_tongping(port.ser_list[dongle_index], [
                         255,255,255,255,255,255] + [4,index,255,255,4,index,255,255])
                        test.tongping_none = [255,255,255,255,255,255] + [4,index,255,255,4,index,255,255]
                    for i in range(len(port.usb_device_sn)):
                        if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
                            test.NO_tongping(port.ser_list[i], [
                             255,255,255,255,255,255] + [4,index,255,255,4,index,255,
                             255])
                            test.device_dan_no_tongping[i] = [4, index, 255, 255]
                        if port.device_info_robobloq[i] == "usb_hid":
                            agreement.colorful_sound_pickup(port.ser_list[i], 2, index)


                index_num = index
                t = threading.Thread(target=(lambda: huancai_qun(index_num)), daemon=True)
                t.start()
            else:
                button_index = self.device_dan_button - 1
                port.devices_ten[self.device_dan_button - 1][5] = index + 1
                port.devices_ten[self.device_dan_button - 1][2] = 0
                port.devices_ten[self.device_dan_button - 1][4] = 0
                port.devices_ten[self.device_dan_button - 1][6] = 0
                port.devices_ten[self.device_dan_button - 1][0] = 1
                test.device_dan_events[self.device_dan_button - 1].set()

                def huancai_dan(index, but_index):
                    time.sleep(0.1)
                    dongle_index = agreement.find_first_index(port.device_info_robobloq, "dongle")
                    if port.device_info_robobloq[but_index] == "dongle":
                        if test.thread_alive("dongle"):
                            test.tongping_none = port.usb_device_sn[but_index][0:6] + [4,index,255,255,255,255,255,255]
                        else:
                            test.NO_tongping(port.ser_list[dongle_index], port.usb_device_sn[but_index][0:6] + [4,index,255,255,255,255,255,255])
                    if port.device_info_robobloq[but_index] == "usb" or port.device_info_robobloq[but_index] == "sound":
                        test.device_dan_no_tongping[but_index] = [
                         4, index, 255, 255]
                        test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[but_index][0:6] + [4,index,255,255,255,255,255,255])
                    if port.device_info_robobloq[but_index] == "usb_hid":
                        agreement.colorful_sound_pickup(port.ser_list[but_index], 2, index)


                index_num = index
                t = threading.Thread(target=(lambda: huancai_dan(index_num, button_index)), daemon=True)
                t.start()
        except:
            pass
        else:
            for i, btn in enumerate(self.shiyin_buttons):
                btn.setChecked(False)
                self.shiyin_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.shiyin_img[i]))))

            for i, btn in enumerate(self.buttons):
                btn.setChecked(False)
                btn.setStyleSheet(f"background-color: rgb{self.colors[i]}; border-radius: 16px;")
# ==== .MainWindow.button_huancai_Clicked.huancai_qun (linha 21509)
time.sleep(0.1)
dongle_index = agreement.find_first_index(port.device_info_robobloq, "dongle")
if "dongle" in port.device_info_robobloq:
    test.NO_tongping(port.ser_list[dongle_index], [
     255,255,255,255,255,255] + [4,index,255,255,4,index,255,255])
    test.tongping_none = [255,255,255,255,255,255] + [4,index,255,255,4,index,255,255]
for i in range(len(port.usb_device_sn)):
    if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
        test.NO_tongping(port.ser_list[i], [
         255,255,255,255,255,255] + [4,index,255,255,4,index,255,
         255])
        test.device_dan_no_tongping[i] = [4, index, 255, 255]
    if port.device_info_robobloq[i] == "usb_hid":
        agreement.colorful_sound_pickup(port.ser_list[i], 2, index)
# ==== .MainWindow.button_huancai_Clicked.huancai_dan (linha 21536)
time.sleep(0.1)
dongle_index = agreement.find_first_index(port.device_info_robobloq, "dongle")
if port.device_info_robobloq[but_index] == "dongle":
    if test.thread_alive("dongle"):
        test.tongping_none = port.usb_device_sn[but_index][0:6] + [4,index,255,255,255,255,255,255]
    else:
        test.NO_tongping(port.ser_list[dongle_index], port.usb_device_sn[but_index][0:6] + [4,index,255,255,255,255,255,255])
if port.device_info_robobloq[but_index] == "usb" or port.device_info_robobloq[but_index] == "sound":
    test.device_dan_no_tongping[but_index] = [
     4, index, 255, 255]
    test.NO_tongping(port.ser_list[but_index], port.usb_device_sn[but_index][0:6] + [4,index,255,255,255,255,255,255])
if port.device_info_robobloq[but_index] == "usb_hid":
    agreement.colorful_sound_pickup(port.ser_list[but_index], 2, index)
# ==== .MainWindow.kaiji_jiyimoshi_changer (linha 21610)
if state:
    port.On_Effect[0] = 255
    agreement.argb_jiyi_on_effect(port.ARGB_ser, 255)
    self.guding_changer_switch.setChecked(False)
    print("记忆模式开关打开")
    self.liusu_fan_lable.hide()
    self.slider_fan_liusu.hide()
    self.liusu_fan_text_lable.hide()
    self.fangxiang_fan_lable.hide()
    for i in self.fan_fangxiang_buttons:
        i.hide()

    self.liangdu_fan_lable.hide()
    self.slider_fan_liangdu.hide()
    self.liangdu_fan_text_lable.hide()
    self.yanse_fan_lable.hide()
    for i in self.fan_dengxiao_buttons:
        i.hide()

    self.button_fan_tiaose.hide()
else:
    port.On_Effect[0] = 1
    self.guding_changer_switch.setChecked(True)
    print("固定模式开关打开")
    agreement.fan_order_list = [0 for i in range(52)]
    print(port.On_Effect[1:17])
    agreement.fan_order_list[0:16] = port.On_Effect[1:17]
    if port.MCU_C_FLAG[0] == 0:
        if port.fan_advanced_effect[13] == 0:
            agreement.argb_jiyi_on_effect(port.ARGB_ser, 1)
            agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
        self.fan_effect_settings(22)
        self.fan_effect_set_sync(agreement.fan_order_list)
port.argb_jiyi_write()
# ==== .MainWindow.kaijizijian_changer (linha 21650)
if state:
    print("开机自检开启")
    port.KaiJi_ZiJian = 255
    agreement.argb_on_off_kaijizijian(port.ARGB_ser, 255)
else:
    print("开机自检关闭")
    port.KaiJi_ZiJian = 1
    agreement.argb_on_off_kaijizijian(port.ARGB_ser, 1)
# ==== .MainWindow.yibu_tongbu_changer (linha 21661)
if state:
    print("积木/联排同步开启")
    port.fan_advanced_effect[19][0] = 1
    self.yibu_six_fan_buttons[0].show()
    self.yibu_six_fan_buttons[1].show()
    self.yibu_six_fan_buttons[2].show()
    self.yibu_six_fan_buttons[3].show()
    self.yibu_six_fan_buttons[4].show()
    self.yibu_six_fan_buttons[5].show()
    fan_id = port.fan_advanced_effect[3]
    effect = port.fan_advanced_effect[4]
    port.fan_advanced_effect[19][1] = fan_id
    for index, vaule in enumerate(self.yibu_ten_fan_buttons):
        if port.ARGB_TEN_LEIXING[index] in port.jimu_lianpai:
            if index != fan_id:
                vaule.hide()
            async_effect_shuju = self.yibu_effect_baocun(fan_id)

    for i in range(10):
        if port.ARGB_TEN_LEIXING[i] in port.jimu_lianpai:
            agreement.fan_order_list = async_effect_shuju
            agreement.ARGB_ten_effect(port.ARGB_ser, 253, i)
        self.yibu_fan_effect_show_hide(effect, fan_id)

else:
    print("积木/联排同步关闭")
    port.fan_advanced_effect[19][0] = 0
    fan_id = port.fan_advanced_effect[19][1]
    for index, vaule in enumerate(self.yibu_ten_fan_buttons):
        if port.ARGB_TEN_LEIXING[index] in port.jimu_lianpai:
            if index != fan_id:
                vaule.show()

    if port.ARGB_TEN_LEIXING[fan_id] == 21:
        for button in self.yibu_six_fan_buttons:
            button.hide()

        port.fan_advanced_effect[19][1] = 255
port.argb_jiyi_write()
# ==== .MainWindow.kaiji_gudingmoshi_changer (linha 21705)
if state:
    self.jiyi_changer_switch.setChecked(False)
    self.kaiji_jiyimoshi_changer(0)
else:
    self.jiyi_changer_switch.setChecked(True)
    self.kaiji_jiyimoshi_changer(1)
# ==== .MainWindow.fan_dan_fangxiang_dengxiao_Clicked (linha 21973)
fan_fangxiang_sender = self.sender()
if fan_fangxiang_sender.isChecked():
    index = self.ARGB_dan_fangxiang_buttons.index(fan_fangxiang_sender)
    print(f"Button {index} is checked")
    print(f"风扇编号: {self.fan_ten_stata}")
    for button in self.ARGB_dan_fangxiang_buttons:
        if button is not fan_fangxiang_sender:
            button.setChecked(False)
        else:
            self.ARGB_dan_fangxiang_buttons[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[index]))))

    for i, btn in enumerate(self.ARGB_dan_fangxiang_buttons):
        if i != index:
            self.ARGB_dan_fangxiang_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[i]))))
        fan_rgb_num = copy.deepcopy(self.fan_ten_stata)
        agreement.ARGB_dan_config(port.ARGB_ser, fan_rgb_num, 2, index)
        port.ARGB_TEN_LIUXIANG[fan_rgb_num] = index
# ==== .MainWindow.fan_fangxiang_dengxiao_Clicked (linha 21995)
effect = self.fan_effect_list.currentIndex()
self.fan_fangxiang_sender = self.sender()
if self.fan_fangxiang_sender.isChecked():
    index = self.fan_fangxiang_buttons.index(self.fan_fangxiang_sender)
    print(f"Button {index} is checked")
    for button in self.fan_fangxiang_buttons:
        if button is not self.fan_fangxiang_sender:
            button.setChecked(False)
        else:
            self.fan_fangxiang_buttons[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[index]))))

    for i, btn in enumerate(self.fan_fangxiang_buttons):
        if i != index:
            self.fan_fangxiang_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[i]))))

    if port.fan_advanced_effect[0] == 4:
        if port.On_Effect[0] == 1:
            agreement.fan_order_list = [0 for i in range(52)]
            port.On_Effect[3] = index
            agreement.fan_order_list[0:16] = port.On_Effect[1:17]
            agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.chuji_effect_changer (linha 22102)
if test.fan_shiyin_event.is_set():
    pass
else:
    test.fan_shiyin_event.set()
    time.sleep(0.05)
self.fan_index_sender = self.sender()
if self.fan_index_sender.isChecked():
    agreement.fan_order_list = [0 for _ in range(52)]
    index = self.chuji_effect_button.index(self.fan_index_sender)
    for i, btn in enumerate(self.chuji_effect_button):
        if i == index:
            self.chuji_effect_button[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.chuji_effect_img_xuanzhong[i]))))
            self.chuji_effect_button_name[i].setStyleSheet("background-color: rgba(255, 255, 255, 0); color: blue; font-size: 15px; ")
        else:
            self.chuji_effect_button[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.chuji_effect_img[i]))))
            self.chuji_effect_button_name[i].setStyleSheet("background-color: rgba(255, 255, 255, 0); color: white; font-size: 15px; ")
            btn.setChecked(False)

    self.fan_effect_settings(port.argb_chuji_effect_no_list[index])
    port.fan_basic_effect = copy.deepcopy(port.argb_chuji_effect_no_list[index])
    port.argb_jiyi_write()
    effect = port.argb_chuji_effect_no_list[index]
    effect_temp = effect
    agreement.fan_order_list[0] = effect_temp
    agreement.fan_order_list[1] = port.ARGB_dan_sync_config_chuji[effect][0]
    agreement.fan_order_list[2] = port.ARGB_dan_sync_config_chuji[effect][1]
    agreement.fan_order_list[3] = port.ARGB_dan_sync_config_chuji[effect][2]
    for i in range(12):
        agreement.fan_order_list[4 + i] = port.ARGB_dan_sync_config_chuji[effect][3 + i]

    agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
    if effect > 10:
        agreement.fan_order_list[0] = effect
    self.fan_effect_set_sync(agreement.fan_order_list)
# ==== .MainWindow.fan_index_clicked (linha 22169)
self.fan_index_sender = self.sender()
if self.fan_index_sender.isChecked():
    index = self.ARGB_FAN_devices.index(self.fan_index_sender)
    print(f"风扇 {index} is checked")
    self.fan_ten_stata = index
    for button in self.ARGB_FAN_devices:
        if button is not self.fan_index_sender:
            button.setChecked(False)
        else:
            self.ARGB_FAN_devices[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.ARGB_FAN_devices_img_xuanzhong[index]))))
            button.setChecked(False)

    for i, btn in enumerate(self.ARGB_FAN_devices):
        if i != index:
            if port.ARGB_TEN_NUM[i] == 0:
                self.ARGB_FAN_devices[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.ARGB_FAN_devices_img_none[i]))))
            else:
                self.ARGB_FAN_devices[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.ARGB_FAN_devices_img[i]))))
        QApplication.processEvents()

    if port.fan_argb_button_model_index == 1:
        if port.fan_advanced_effect[0] == 4:
            self.fan_ten_stata = index
            self.fan_set_config(index)
            if port.ARGB_TEN_NUM[index] == 0:
                self.ARGB_number_lable.show()
                self.ARGB_settings_box_number.show()
            else:
                self.ARGB_number_lable.hide()
                self.ARGB_settings_box_number.hide()
            self.ARGB_number_lable.show()
            self.ARGB_settings_box_number.show()
            if port.MCU_C_FLAG[0] == 0:
                if port.fan_advanced_effect[13] == 0:
                    agreement.argb_fan_set(port.ARGB_ser, index)
# ==== .MainWindow.fan_tese_clicked (linha 22485)
self.fan_tese_sender = self.sender()
if self.fan_tese_sender.isChecked():
    index = self.fan_buttons_tese.index(self.fan_tese_sender)
    port.fan_advanced_effect[2] = 2
    port.fan_advanced_effect[3] = index
    print(f"Button {index} is checked")
    if index == 0:
        self.tese_tongping_shiyin_xianshi(1)
    elif index == 1:
        self.tese_tongping_shiyin_xianshi(2)
    effect = 0
    if index == 0:
        effect = 20
    elif index == 1:
        effect = 21
    self.fan_effect_settings(effect)
    effect_id = [effect] + port.ARGB_qun_async_config[effect]
    self.fan_effect_set_sync(effect_id)
    test.fan_screen_shiyin_effect = index
    test.fan_shiyin_event.set()
    time.sleep(0.08)
    test.fan_shiyin_event.clear()
    t = threading.Thread(target=(lambda: test.ARGB_FAN_sound()), daemon=True)
    t.start()
    port.write_argb_fan_all()
    port.argb_jiyi_write()
    for i in range(10):
        self.ARGB_FAN_devices[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.ARGB_FAN_devices_img_xuanzhong[i]))))
        self.ARGB_FAN_devices[i].setChecked(False)

    for i in range(6):
        self.ARGB_FAN_chuan[i].hide()

    self.ARGB_FAN_all.setStyleSheet("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
    self.ARGB_FAN_all.setChecked(False)
    self.ARGB_FAN_sync_all.setStyleSheet("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
    self.ARGB_FAN_sync_all.setChecked(False)
# ==== .MainWindow.tongbu_yibu_hebing_zhuansu_button_Clicked (linha 22820)
sender = self.sender()
index = self.fan_effect_speed_buttons.index(sender)
self.fan_effect_speed_buttons[index].setChecked(True)
if sender.isChecked():
    for i, button in enumerate(self.fan_effect_speed_buttons):
        if button is not sender:
            button.setChecked(False)
        self.tongbu_effect_widget.hide()
        self.yibu_effect_widget.hide()
        self.hebing_effect_widget.hide()
        self.fan_speed_ten_widget.hide()
        self.new_shezhi_yemian_widget.hide()
        self.tongbu_dengguang_on_off.show()
        self.tongbu_dengguang_on_off_switch.show()
        self.tongbu_dengguang_on_off_lable.show()
        self.tongbu_speed_on_off.hide()
        self.tongbu_speed_on_off_switch.hide()
        self.tongbu_speed_on_off_lable.hide()
        self.yibu_baocun_widget.hide()
        self.kaiguandeng_zhanwei.show()
        self.kaiguandeng_switch.show()
        self.kaiguandeng_lable_txt.show()

    if port.fan_advanced_effect[13] == 0:
        self.kaiguandeng_shuxian.show()
port.fan_advanced_effect[0] = index
if index < 3 or index == 4:
    if port.MCU_C_FLAG[0] == 1:
        self.overlay.show()
        self.overlay.raise_()
    else:
        self.overlay.hide()
    if port.fan_advanced_effect[13] == 1:
        self.overlay.show()
        self.overlay.raise_()
        agreement.argb_fan_light_off(port.ARGB_ser)
        self.tongbu_dengguang_on_off.hide()
        self.tongbu_dengguang_on_off_switch.hide()
        self.tongbu_dengguang_on_off_lable.hide()
    if index == 3:
        if port.MCU_C_FLAG[1] == 1:
            self.overlay.show()
            self.overlay.raise_()
        else:
            self.overlay.hide()
    if index == 0:
        print("同步灯效")
        self.tongbu_effect_widget.show()
        port.fan_advanced_effect[1] = 0
        index_tongbu = port.fan_basic_effect
        self.tongbu_effect_buttons[index_tongbu].click()
    if index == 1:
        print("异步灯效")
        self.yibu_effect_widget.show()
        self.yibu_baocun_widget.show()
        if port.PID_COMPAY == 5 or port.PID_COMPAY == 0:
            self.rebuild_cards()
        fan_id = port.fan_advanced_effect[3]
        if port.ARGB_TEN_NUM[fan_id] == 0:
            for id in range(10):
                if port.ARGB_TEN_NUM[id] != 0:
                    fan_id = id
                self.yibu_ten_fan_buttons[fan_id].click()

            if index == 2:
                print("合并灯效")
                self.hebing_effect_widget.show()
                port.fan_advanced_effect[1] = 2
                index_hebing = port.fan_advanced_effect[2]
                self.hebing_effect_buttons[index_hebing].click()
            if index == 3:
                print("风扇转速")
                self.tongbu_speed_on_off.show()
                self.tongbu_speed_on_off_switch.show()
                self.tongbu_speed_on_off_lable.show()
                self.fan_speed_ten_widget.show()
                on_off = port.fan_advanced_effect[6]
                self.zidong_fan_speed_onoff(on_off)
                if on_off == 1:
                    self.zidongtiaosu_changer_switch.setChecked(True)
                else:
                    self.zidongtiaosu_changer_switch.setChecked(False)
                if port.MCU_C_FLAG[1] == 1:
                    self.tongbu_speed_on_off_switch.setChecked(True)
                    agreement.argb_cp_mcu_fan_speed(port.ARGB_ser, 0)
                else:
                    self.tongbu_speed_on_off_switch.setChecked(False)
                    agreement.argb_cp_mcu_fan_speed(port.ARGB_ser, 1)
                self.kaiguandeng_shuxian.hide()
                self.kaiguandeng_zhanwei.hide()
                self.kaiguandeng_switch.hide()
                self.kaiguandeng_lable_txt.hide()
                self.tongbu_dengguang_on_off.hide()
                self.tongbu_dengguang_on_off_switch.hide()
                self.tongbu_dengguang_on_off_lable.hide()
            if index == 4:
                print("控制器设置")
                self.new_shezhi_yemian_widget.show()
                if port.ARGB_433RF[0] == 1:
                    self.ARGB_rf_peidui_zhuangtai.setText(self.rf_433_peidui[2])
                    self.ARGB_rf_peidui_sn.setText(f'{"".join([f"{x:02X}" for x in port.ARGB_433RF[1:3]])}')
                    self.rf_433_buttons[0].hide()
                    self.rf_433_buttons[1].show()
                    self.rf_433_buttons[2].hide()
                else:
                    self.ARGB_rf_peidui_zhuangtai.setText(self.rf_433_peidui[0])
                    self.ARGB_rf_peidui_sn.setText(self.rf_433_peidui[0])
                    self.rf_433_buttons[1].hide()
                    self.rf_433_buttons[0].show()
                    self.rf_433_buttons[2].hide()
                self.ARGB_RF_PEIDUI_FLAG = 0
                port.ARGB_433RF_peidui_flag = 0
                for index, num in enumerate(port.ARGB_TEN_NUM):
                    if index != 0:
                        if num != 0:
                            self.ARGB_FAN_devices[index].click()
                            break
                        print(f"port.On_Effect[0] : {port.On_Effect[0]}")

                if port.On_Effect[0] == 255:
                    self.kaiji_jiyimoshi_changer(1)
                    self.jiyi_changer_switch.setChecked(True)
                if port.On_Effect[0] == 1:
                    self.kaiji_jiyimoshi_changer(0)
                    self.guding_changer_switch.setChecked(True)
                if port.KaiJi_ZiJian == 1:
                    self.kaijizijian_changer_switch.setChecked(False)
                elif port.KaiJi_ZiJian == 255:
                    self.kaijizijian_changer_switch.setChecked(True)
                if port.PID_COMPAY == 5 or port.PID_COMPAY == 7:
                    self.ARGB_rf_lable.hide()
                    self.ARGB_rf_peidui_zhuangtai.hide()
                    self.ARGB_rf_peidui_sn_config.hide()
                    self.ARGB_rf_peidui_sn.hide()
                    self.rf_433_buttons[0].hide()
                    self.rf_433_buttons[1].hide()
                    self.rf_433_buttons[2].hide()
    port.argb_jiyi_write()
# ==== .MainWindow.fan_dengxiao_index (linha 23039)
# descompilação falhou (demorou demais); desmontagem:
# # pydisasm version 6.1.7
# # Python bytecode 3.8.0 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       fan_dengxiao_index
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  17
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        23039
# # Constants:
# #    0: None
# #    1: '选项卡切换'
# #    2: <Code38 code object <listcomp> at 0x2589185a430, file Sync_light.py>, line 23042
# #    3: 'MainWindow.fan_dengxiao_index.<locals>.<listcomp>'
# #    4: 52
# #    5: 0
# #    6: 1
# #    7: True
# #    8: 13
# #    9: '关灯'
# #   10: 'en'
# #   11: 'Turn Off'
# #   12: '\n            background-color: rgba(255, 255, 255, 0);\n            color: rgba(255, 255, 255, 153);\n            font-size: 16px; \n                                                                                                \n            '
# #   13: False
# #   14: '系统信息选项卡'
# #   15: 'ARGB控制器选项卡'
# #   16: 0.01
# #   17: 3
# #   18: 4
# #   19: 2
# #   20: 160
# #   21: '间隔次数：'
# #   22: ', 灯珠总数：'
# #   23: '幕布灯效选项卡'
# #   24: '设置选项卡'
# #   25: 5
# #   26: '高级灯效选项卡 '
# #   27: 'zh'
# #   28: 10
# #   29: 6
# #   30: 255
# #   31: '异步'
# #   32: 'background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;'
# #   33: 'background-color: rgb(25,78,255);color: rgb(243,244,243);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;'
# #   34: 15
# #   35: 254
# #   36: 20
# #   37: 21
# #   38: <Code38 code object <lambda> at 0x2589185a890, file Sync_light.py>, line 23340
# #   39: 'MainWindow.fan_dengxiao_index.<locals>.<lambda>'
# #   40: ('target', 'daemon')
# #   41: 'fan_id : '
# #   42: 12
# #   43: '硬件设置选项卡'
# #   44: <Code38 code object <listcomp> at 0x2589185a7b0, file Sync_light.py>, line 23503
# #   45: 17
# #   46: 16
# #   47: 22
# #   48: "遥控器配对状态: <span style='color:red;'>已配对</span>  配对码信息： "
# #   49: ''
# #   50: <Code38 code object <listcomp> at 0x2589185a970, file Sync_light.py>, line 23552
# #   51: '  '
# #   52: "Remote pairing status: <span style='color:red;'>Paired</span>  Pairing code information: "
# #   53: <Code38 code object <listcomp> at 0x2589185aa50, file Sync_light.py>, line 23554
# #   54: '遥控器配对状态: 未配对  配对码信息：      '
# #   55: 'Remote pairing status: Unpaired  Pairing code information:      '
# # Names:
# #    0: print
# #    1: range
# #    2: agreement
# #    3: fan_order_list
# #    4: port
# #    5: FAN_LIGHT_FLAG
# #    6: light_on_off_switch
# #    7: setChecked
# #    8: system_info_widget
# #    9: hide
# #   10: tongbu_dengguang_on_off
# #   11: tongbu_dengguang_on_off_switch
# #   12: tongbu_dengguang_on_off_lable
# #   13: tongbu_effect_widget
# #   14: yibu_effect_widget
# #   15: hebing_effect_widget
# #   16: fan_speed_ten_widget
# #   17: new_shezhi_yemian_widget
# #   18: effect_fan_speed_widget
# #   19: new_shezhi_widget
# #   20: shezhi_tongyong_widget
# #   21: shezhi_zhichi_widget
# #   22: tongbu_speed_on_off
# #   23: tongbu_speed_on_off_switch
# #   24: tongbu_speed_on_off_lable
# #   25: top_zhanwei_widget
# #   26: kaiguandeng_shuxian
# #   27: kaiguandeng_zhanwei
# #   28: kaiguandeng_switch
# #   29: kaiguandeng_lable_txt
# #   30: xitongxianshi_on_off_lable
# #   31: xitongxianshi_on_off_switch
# #   32: yibu_baocun_widget
# #   33: fan_advanced_effect
# #   34: setText
# #   35: language
# #   36: setStyleSheet
# #   37: fan_argb_button_model_index
# #   38: show
# #   39: main_widget_mubu
# #   40: effect_list_label
# #   41: fan_effect_list
# #   42: ARGB_FAN_sync_all
# #   43: ARGB_FAN_all
# #   44: chuji_effect_widget
# #   45: fan_lable_tese
# #   46: overlay
# #   47: _last_future
# #   48: done
# #   49: time
# #   50: sleep
# #   51: fan_config_dan_widget
# #   52: MCU_C_FLAG
# #   53: raise_
# #   54: fan_effect_speed_buttons
# #   55: click
# #   56: ARGB_MCU_C_changer
# #   57: ARGB_ser
# #   58: argb_fan_light_off
# #   59: sum
# #   60: ARGB_TEN_NUM
# #   61: canvas
# #   62: scene
# #   63: send_flag_devices_fan
# #   64: load_icons_state
# #   65: update
# #   66: on_effect_changed
# #   67: effect_manager
# #   68: current_effect
# #   69: new_shezhi_buttons
# #   70: liusu_fan_lable
# #   71: slider_fan_liusu
# #   72: liusu_fan_text_lable
# #   73: fangxiang_fan_lable
# #   74: fan_fangxiang_buttons
# #   75: liangdu_fan_lable
# #   76: slider_fan_liangdu
# #   77: liangdu_fan_text_lable
# #   78: yanse_fan_lable
# #   79: fan_dengxiao_buttons
# #   80: button_fan_tiaose
# #   81: blockSignals
# #   82: clear
# #   83: dengxiao_model2_zh
# #   84: dengxiao_model2_en
# #   85: addItems
# #   86: setCurrentIndex
# #   87: ARGB_FAN_devices
# #   88: ARGB_FAN_chuan
# #   89: tese_tongping_shiyin_xianshi
# #   90: fan_effect_settings
# #   91: anyc_list
# #   92: setIcon
# #   93: QIcon
# #   94: file_path_img
# #   95: ARGB_FAN_devices_img_xuanzhong
# #   96: ARGB_send_show_effect
# #   97: ARGB_qun_async_config
# #   98: ARGB_ten_effect
# #   99: fan_effect_set_sync
# #  100: test
# #  101: fan_screen_shiyin_effect
# #  102: fan_shiyin_event
# #  103: set
# #  104: threading
# #  105: Thread
# #  106: start
# #  107: ARGB_FAN_devices_img
# #  108: copy
# #  109: deepcopy
# #  110: ARGB_TEN_MODEL1
# #  111: gaoji_ten_dengxiao_leixing
# #  112: ARGB_TEN_LEIXING
# #  113: jimu_lianpai
# #  114: ARGB_FAN_chuan_img
# #  115: ARGB_FAN_chuan_img_xuanzhong
# #  116: enumerate
# #  117: dan_fan_huifu_tab_effect
# #  118: ARGB_dan_sync_config_gaoji
# #  119: ARGB_FAN_SN_NUM
# #  120: jiyi_moshi
# #  121: jiyi_changer_switch
# #  122: guding_moshi
# #  123: guding_changer_switch
# #  124: kaijizijian
# #  125: kaijizijian_changer_switch
# #  126: KaiJi_ZiJian
# #  127: On_Effect
# #  128: ARGB_FAN_devices_img_none
# #  129: argb_fan_set
# #  130: fan_set_config
# #  131: ARGB_433RF
# #  132: ARGB_rf_lable
# #  133: join
# #  134: rf_433_buttons
# #  135: fan_buttons_tese
# # Varnames:
# #	self, i, vaule_argb, sum_rgb, C_NUM, sum_send_z, sum_send_y, effect, fan_index, async_index, tongp_shiy_index, dengxiao, effect_id, t, index, fan_id, vaule
# # Positional arguments:
# #	self
# # Local variables:
# #    1: i
# #    2: vaule_argb
# #    3: sum_rgb
# #    4: C_NUM
# #    5: sum_send_z
# #    6: sum_send_y
# #    7: effect
# #    8: fan_index
# #    9: async_index
# #   10: tongp_shiy_index
# #   11: dengxiao
# #   12: effect_id
# #   13: t
# #   14: index
# #   15: fan_id
# #   16: vaule
# 
# # Method Name:       fan_dengxiao_index
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  17
# # Stack size:        10
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        23039
# # Constants:
# #    0: None
# #    1: '选项卡切换'
# #    2: <Code38 code object <listcomp> at 0x2589185a430, file Sync_light.py>, line 23042
# #    3: 'MainWindow.fan_dengxiao_index.<locals>.<listcomp>'
# #    4: 52
# #    5: 0
# #    6: 1
# #    7: True
# #    8: 13
# #    9: '关灯'
# #   10: 'en'
# #   11: 'Turn Off'
# #   12: '\n            background-color: rgba(255, 255, 255, 0);\n            color: rgba(255, 255, 255, 153);\n            font-size: 16px; \n                                                                                                \n            '
# #   13: False
# #   14: '系统信息选项卡'
# #   15: 'ARGB控制器选项卡'
# #   16: 0.01
# #   17: 3
# #   18: 4
# #   19: 2
# #   20: 160
# #   21: '间隔次数：'
# #   22: ', 灯珠总数：'
# #   23: '幕布灯效选项卡'
# #   24: '设置选项卡'
# #   25: 5
# #   26: '高级灯效选项卡 '
# #   27: 'zh'
# #   28: 10
# #   29: 6
# #   30: 255
# #   31: '异步'
# #   32: 'background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;'
# #   33: 'background-color: rgb(25,78,255);color: rgb(243,244,243);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;'
# #   34: 15
# #   35: 254
# #   36: 20
# #   37: 21
# #   38: <Code38 code object <lambda> at 0x2589185a890, file Sync_light.py>, line 23340
# #   39: 'MainWindow.fan_dengxiao_index.<locals>.<lambda>'
# #   40: ('target', 'daemon')
# #   41: 'fan_id : '
# #   42: 12
# #   43: '硬件设置选项卡'
# #   44: <Code38 code object <listcomp> at 0x2589185a7b0, file Sync_light.py>, line 23503
# #   45: 17
# #   46: 16
# #   47: 22
# #   48: "遥控器配对状态: <span style='color:red;'>已配对</span>  配对码信息： "
# #   49: ''
# #   50: <Code38 code object <listcomp> at 0x2589185a970, file Sync_light.py>, line 23552
# #   51: '  '
# #   52: "Remote pairing status: <span style='color:red;'>Paired</span>  Pairing code information: "
# #   53: <Code38 code object <listcomp> at 0x2589185aa50, file Sync_light.py>, line 23554
# #   54: '遥控器配对状态: 未配对  配对码信息：      '
# #   55: 'Remote pairing status: Unpaired  Pairing code information:      '
# # Names:
# #    0: print
# #    1: range
# #    2: agreement
# #    3: fan_order_list
# #    4: port
# #    5: FAN_LIGHT_FLAG
# #    6: light_on_off_switch
# #    7: setChecked
# #    8: system_info_widget
# #    9: hide
# #   10: tongbu_dengguang_on_off
# #   11: tongbu_dengguang_on_off_switch
# #   12: tongbu_dengguang_on_off_lable
# #   13: tongbu_effect_widget
# #   14: yibu_effect_widget
# #   15: hebing_effect_widget
# #   16: fan_speed_ten_widget
# #   17: new_shezhi_yemian_widget
# #   18: effect_fan_speed_widget
# #   19: new_shezhi_widget
# #   20: shezhi_tongyong_widget
# #   21: shezhi_zhichi_widget
# #   22: tongbu_speed_on_off
# #   23: tongbu_speed_on_off_switch
# #   24: tongbu_speed_on_off_lable
# #   25: top_zhanwei_widget
# #   26: kaiguandeng_shuxian
# #   27: kaiguandeng_zhanwei
# #   28: kaiguandeng_switch
# #   29: kaiguandeng_lable_txt
# #   30: xitongxianshi_on_off_lable
# #   31: xitongxianshi_on_off_switch
# #   32: yibu_baocun_widget
# #   33: fan_advanced_effect
# #   34: setText
# #   35: language
# #   36: setStyleSheet
# #   37: fan_argb_button_model_index
# #   38: show
# #   39: main_widget_mubu
# #   40: effect_list_label
# #   41: fan_effect_list
# #   42: ARGB_FAN_sync_all
# #   43: ARGB_FAN_all
# #   44: chuji_effect_widget
# #   45: fan_lable_tese
# #   46: overlay
# #   47: _last_future
# #   48: done
# #   49: time
# #   50: sleep
# #   51: fan_config_dan_widget
# #   52: MCU_C_FLAG
# #   53: raise_
# #   54: fan_effect_speed_buttons
# #   55: click
# #   56: ARGB_MCU_C_changer
# #   57: ARGB_ser
# #   58: argb_fan_light_off
# #   59: sum
# #   60: ARGB_TEN_NUM
# #   61: canvas
# #   62: scene
# #   63: send_flag_devices_fan
# #   64: load_icons_state
# #   65: update
# #   66: on_effect_changed
# #   67: effect_manager
# #   68: current_effect
# #   69: new_shezhi_buttons
# #   70: liusu_fan_lable
# #   71: slider_fan_liusu
# #   72: liusu_fan_text_lable
# #   73: fangxiang_fan_lable
# #   74: fan_fangxiang_buttons
# #   75: liangdu_fan_lable
# #   76: slider_fan_liangdu
# #   77: liangdu_fan_text_lable
# #   78: yanse_fan_lable
# #   79: fan_dengxiao_buttons
# #   80: button_fan_tiaose
# #   81: blockSignals
# #   82: clear
# #   83: dengxiao_model2_zh
# #   84: dengxiao_model2_en
# #   85: addItems
# #   86: setCurrentIndex
# #   87: ARGB_FAN_devices
# #   88: ARGB_FAN_chuan
# #   89: tese_tongping_shiyin_xianshi
# #   90: fan_effect_settings
# #   91: anyc_list
# #   92: setIcon
# #   93: QIcon
# #   94: file_path_img
# #   95: ARGB_FAN_devices_img_xuanzhong
# #   96: ARGB_send_show_effect
# #   97: ARGB_qun_async_config
# #   98: ARGB_ten_effect
# #   99: fan_effect_set_sync
# #  100: test
# #  101: fan_screen_shiyin_effect
# #  102: fan_shiyin_event
# #  103: set
# #  104: threading
# #  105: Thread
# #  106: start
# #  107: ARGB_FAN_devices_img
# #  108: copy
# #  109: deepcopy
# #  110: ARGB_TEN_MODEL1
# #  111: gaoji_ten_dengxiao_leixing
# #  112: ARGB_TEN_LEIXING
# #  113: jimu_lianpai
# #  114: ARGB_FAN_chuan_img
# #  115: ARGB_FAN_chuan_img_xuanzhong
# #  116: enumerate
# #  117: dan_fan_huifu_tab_effect
# #  118: ARGB_dan_sync_config_gaoji
# #  119: ARGB_FAN_SN_NUM
# #  120: jiyi_moshi
# #  121: jiyi_changer_switch
# #  122: guding_moshi
# #  123: guding_changer_switch
# #  124: kaijizijian
# #  125: kaijizijian_changer_switch
# #  126: KaiJi_ZiJian
# #  127: On_Effect
# #  128: ARGB_FAN_devices_img_none
# #  129: argb_fan_set
# #  130: fan_set_config
# #  131: ARGB_433RF
# #  132: ARGB_rf_lable
# #  133: join
# #  134: rf_433_buttons
# #  135: fan_buttons_tese
# # Varnames:
# #	self, i, vaule_argb, sum_rgb, C_NUM, sum_send_z, sum_send_y, effect, fan_index, async_index, tongp_shiy_index, dengxiao, effect_id, t, index, fan_id, vaule
# # Positional arguments:
# #	self
# # Local variables:
# #    1: i
# #    2: vaule_argb
# #    3: sum_rgb
# #    4: C_NUM
# #    5: sum_send_z
# #    6: sum_send_y
# #    7: effect
# #    8: fan_index
# #    9: async_index
# #   10: tongp_shiy_index
# #   11: dengxiao
# #   12: effect_id
# #   13: t
# #   14: index
# #   15: fan_id
# #   16: vaule
# 23040:           0 LOAD_GLOBAL          (print)
#                2 LOAD_CONST           ("选项卡切换")
#                4 CALL_FUNCTION        (1 positional argument)
#                6 POP_TOP
# 
# 23042:           8 LOAD_CONST           (<Code38 code object <listcomp> at 0x2589185a430, file Sync_light.py>, line 23042)
#               10 LOAD_CONST           ("MainWindow.fan_dengxiao_index.<locals>.<listcomp>")
#               12 MAKE_FUNCTION        (No arguments)
#               14 LOAD_GLOBAL          (range)
#               16 LOAD_CONST           (52)
#               18 CALL_FUNCTION        (1 positional argument)
#               20 GET_ITER
#               22 CALL_FUNCTION        (1 positional argument)
#               24 LOAD_GLOBAL          (agreement)
#               26 STORE_ATTR           (fan_order_list)
# 
# 23050:          28 LOAD_GLOBAL          (port)
#               30 LOAD_ATTR            (FAN_LIGHT_FLAG)
#               32 LOAD_CONST           (0)
#               34 COMPARE_OP           (==)
#               36 POP_JUMP_IF_FALSE    (to 56)
# 
# 23051:          38 LOAD_CONST           (1)
#               40 LOAD_GLOBAL          (port)
#               42 STORE_ATTR           (FAN_LIGHT_FLAG)
# 
# 23056:          44 LOAD_FAST            (self)
#               46 LOAD_ATTR            (light_on_off_switch)
#               48 LOAD_METHOD          (setChecked)
#               50 LOAD_CONST           (True)
#               52 CALL_METHOD          (1 positional argument)
#               54 POP_TOP
# 
# 23059:     >>   56 LOAD_FAST            (self)
#               58 LOAD_ATTR            (system_info_widget)
#               60 LOAD_METHOD          (hide)
#               62 CALL_METHOD          (0 positional arguments)
#               64 POP_TOP
# 
# 23061:          66 LOAD_FAST            (self)
#               68 LOAD_ATTR            (tongbu_dengguang_on_off)
#               70 LOAD_METHOD          (hide)
#               72 CALL_METHOD          (0 positional arguments)
#               74 POP_TOP
# 
# 23062:          76 LOAD_FAST            (self)
#               78 LOAD_ATTR            (tongbu_dengguang_on_off_switch)
#               80 LOAD_METHOD          (hide)
#               82 CALL_METHOD          (0 positional arguments)
#               84 POP_TOP
# 
# 23063:          86 LOAD_FAST            (self)
#               88 LOAD_ATTR            (tongbu_dengguang_on_off_lable)
#               90 LOAD_METHOD          (hide)
#               92 CALL_METHOD          (0 positional arguments)
#               94 POP_TOP
# 
# 23064:          96 LOAD_FAST            (self)
#               98 LOAD_ATTR            (tongbu_effect_widget)
#              100 LOAD_METHOD          (hide)
#              102 CALL_METHOD          (0 positional arguments)
#              104 POP_TOP
# 
# 23065:         106 LOAD_FAST            (self)
#              108 LOAD_ATTR            (yibu_effect_widget)
#              110 LOAD_METHOD          (hide)
#              112 CALL_METHOD          (0 positional arguments)
#              114 POP_TOP
# 
# 23066:         116 LOAD_FAST            (self)
#              118 LOAD_ATTR            (hebing_effect_widget)
#              120 LOAD_METHOD          (hide)
#              122 CALL_METHOD          (0 positional arguments)
#              124 POP_TOP
# 
# 23067:         126 LOAD_FAST            (self)
#              128 LOAD_ATTR            (fan_speed_ten_widget)
#              130 LOAD_METHOD          (hide)
#              132 CALL_METHOD          (0 positional arguments)
#              134 POP_TOP
# 
# 23068:         136 LOAD_FAST            (self)
#              138 LOAD_ATTR            (new_shezhi_yemian_widget)
#              140 LOAD_METHOD          (hide)
#              142 CALL_METHOD          (0 positional arguments)
#              144 POP_TOP
# 
# 23069:         146 LOAD_FAST            (self)
#              148 LOAD_ATTR            (effect_fan_speed_widget)
#              150 LOAD_METHOD          (hide)
#              152 CALL_METHOD          (0 positional arguments)
#              154 POP_TOP
# 
# 23070:         156 LOAD_FAST            (self)
#              158 LOAD_ATTR            (new_shezhi_widget)
#              160 LOAD_METHOD          (hide)
#              162 CALL_METHOD          (0 positional arguments)
#              164 POP_TOP
# 
# 23071:         166 LOAD_FAST            (self)
#              168 LOAD_ATTR            (shezhi_tongyong_widget)
#              170 LOAD_METHOD          (hide)
#              172 CALL_METHOD          (0 positional arguments)
#              174 POP_TOP
# 
# 23072:         176 LOAD_FAST            (self)
#              178 LOAD_ATTR            (shezhi_zhichi_widget)
#              180 LOAD_METHOD          (hide)
#              182 CALL_METHOD          (0 positional arguments)
#              184 POP_TOP
# 
# 23075:         186 LOAD_FAST            (self)
#              188 LOAD_ATTR            (tongbu_dengguang_on_off)
#              190 LOAD_METHOD          (hide)
#              192 CALL_METHOD          (0 positional arguments)
#              194 POP_TOP
# 
# 23076:         196 LOAD_FAST            (self)
#              198 LOAD_ATTR            (tongbu_dengguang_on_off_switch)
#              200 LOAD_METHOD          (hide)
#              202 CALL_METHOD          (0 positional arguments)
#              204 POP_TOP
# 
# 23077:         206 LOAD_FAST            (self)
#              208 LOAD_ATTR            (tongbu_dengguang_on_off_lable)
#              210 LOAD_METHOD          (hide)
#              212 CALL_METHOD          (0 positional arguments)
#              214 POP_TOP
# 
# 23079:         216 LOAD_FAST            (self)
#              218 LOAD_ATTR            (tongbu_speed_on_off)
#              220 LOAD_METHOD          (hide)
#              222 CALL_METHOD          (0 positional arguments)
#              224 POP_TOP
# 
# 23080:         226 LOAD_FAST            (self)
#              228 LOAD_ATTR            (tongbu_speed_on_off_switch)
#              230 LOAD_METHOD          (hide)
#              232 CALL_METHOD          (0 positional arguments)
#              234 POP_TOP
# 
# 23081:         236 LOAD_FAST            (self)
#              238 LOAD_ATTR            (tongbu_speed_on_off_lable)
#              240 LOAD_METHOD          (hide)
#              242 CALL_METHOD          (0 positional arguments)
#              244 POP_TOP
# 
# 23083:         246 LOAD_FAST            (self)
#              248 LOAD_ATTR            (top_zhanwei_widget)
#              250 LOAD_METHOD          (hide)
#              252 CALL_METHOD          (0 positional arguments)
#              254 POP_TOP
# 
# 23084:         256 LOAD_FAST            (self)
#              258 LOAD_ATTR            (kaiguandeng_shuxian)
#              260 LOAD_METHOD          (hide)
#              262 CALL_METHOD          (0 positional arguments)
#              264 POP_TOP
# 
# 23085:         266 LOAD_FAST            (self)
#              268 LOAD_ATTR            (kaiguandeng_zhanwei)
#              270 LOAD_METHOD          (hide)
#              272 CALL_METHOD          (0 positional arguments)
#              274 POP_TOP
# 
# 23086:         276 LOAD_FAST            (self)
#              278 LOAD_ATTR            (kaiguandeng_switch)
#              280 LOAD_METHOD          (hide)
#              282 CALL_METHOD          (0 positional arguments)
#              284 POP_TOP
# 
# 23087:         286 LOAD_FAST            (self)
#              288 LOAD_ATTR            (kaiguandeng_lable_txt)
#              290 LOAD_METHOD          (hide)
#              292 CALL_METHOD          (0 positional arguments)
#              294 POP_TOP
# 
# 23088:         296 LOAD_FAST            (self)
#              298 LOAD_ATTR            (xitongxianshi_on_off_lable)
#              300 LOAD_METHOD          (hide)
#              302 CALL_METHOD          (0 positional arguments)
#              304 POP_TOP
# 
# 23089:         306 LOAD_FAST            (self)
#              308 LOAD_ATTR            (xitongxianshi_on_off_switch)
#              310 LOAD_METHOD          (hide)
#              312 CALL_METHOD          (0 positional arguments)
#              314 POP_TOP
# 
# 23091:         316 LOAD_FAST            (self)
#              318 LOAD_ATTR            (yibu_baocun_widget)
#              320 LOAD_METHOD          (hide)
#              322 CALL_METHOD          (0 positional arguments)
#              324 POP_TOP
# 
# 23093:         326 LOAD_GLOBAL          (port)
#              328 LOAD_ATTR            (fan_advanced_effect)
#              330 LOAD_CONST           (13)
#              332 BINARY_SUBSCR
#              334 LOAD_CONST           (1)
#              336 COMPARE_OP           (==)
#              338 EXTENDED_ARG         (256)
#              340 POP_JUMP_IF_FALSE    (to 402)
# 
# 23094:         342 LOAD_FAST            (self)
#              344 LOAD_ATTR            (kaiguandeng_lable_txt)
#              346 LOAD_METHOD          (setText)
#              348 LOAD_CONST           ("关灯")
#              350 CALL_METHOD          (1 positional argument)
#              352 POP_TOP
# 
# 23095:         354 LOAD_GLOBAL          (port)
#              356 LOAD_ATTR            (language)
#              358 LOAD_CONST           ("en")
#              360 COMPARE_OP           (==)
#              362 EXTENDED_ARG         (256)
#              364 POP_JUMP_IF_FALSE    (to 378)
# 
# 23096:         366 LOAD_FAST            (self)
#              368 LOAD_ATTR            (kaiguandeng_lable_txt)
#              370 LOAD_METHOD          (setText)
#              372 LOAD_CONST           ("Turn Off")
#              374 CALL_METHOD          (1 positional argument)
#              376 POP_TOP
# 
# 23097:     >>  378 LOAD_FAST            (self)
#              380 LOAD_ATTR            (kaiguandeng_lable_txt)
#              382 LOAD_METHOD          (setStyleSheet)
#              384 LOAD_CONST           ("\n            background-color: rgba(255, 255, 255, 0);\n            color: rgba(255, 255, 255, 153);\n            font-size: 16px; \n                                                                                                \n            ")
#              386 CALL_METHOD          (1 positional argument)
#              388 POP_TOP
# 
# 23103:         390 LOAD_FAST            (self)
#              392 LOAD_ATTR            (kaiguandeng_switch)
#              394 LOAD_METHOD          (setChecked)
#              396 LOAD_CONST           (False)
#              398 CALL_METHOD          (1 positional argument)
#              400 POP_TOP
# 
# 23105:     >>  402 LOAD_GLOBAL          (port)
#              404 LOAD_ATTR            (fan_argb_button_model_index)
#              406 LOAD_CONST           (0)
#              408 COMPARE_OP           (==)
#              410 EXTENDED_ARG         (512)
#              412 POP_JUMP_IF_FALSE    (to 554)
# 
# 23106:         414 LOAD_GLOBAL          (print)
#              416 LOAD_CONST           ("系统信息选项卡")
#              418 CALL_FUNCTION        (1 positional argument)
#              420 POP_TOP
# 
# 23107:         422 LOAD_FAST            (self)
#              424 LOAD_ATTR            (xitongxianshi_on_off_lable)
#              426 LOAD_METHOD          (show)
#              428 CALL_METHOD          (0 positional arguments)
#              430 POP_TOP
# 
# 23108:         432 LOAD_FAST            (self)
#              434 LOAD_ATTR            (xitongxianshi_on_off_switch)
#              436 LOAD_METHOD          (show)
#              438 CALL_METHOD          (0 positional arguments)
#              440 POP_TOP
# 
# 23109:         442 LOAD_FAST            (self)
#              444 LOAD_ATTR            (system_info_widget)
#              446 LOAD_METHOD          (show)
#              448 CALL_METHOD          (0 positional arguments)
#              450 POP_TOP
# 
# 23110:         452 LOAD_FAST            (self)
#              454 LOAD_ATTR            (main_widget_mubu)
#              456 LOAD_METHOD          (hide)
#              458 CALL_METHOD          (0 positional arguments)
#              460 POP_TOP
# 
# 23111:         462 LOAD_FAST            (self)
#              464 LOAD_ATTR            (effect_list_label)
#              466 LOAD_METHOD          (hide)
#              468 CALL_METHOD          (0 positional arguments)
#              470 POP_TOP
# 
# 23112:         472 LOAD_FAST            (self)
#              474 LOAD_ATTR            (fan_effect_list)
#              476 LOAD_METHOD          (hide)
#              478 CALL_METHOD          (0 positional arguments)
#              480 POP_TOP
# 
# 23113:         482 LOAD_FAST            (self)
#              484 LOAD_ATTR            (ARGB_FAN_sync_all)
#              486 LOAD_METHOD          (hide)
#              488 CALL_METHOD          (0 positional arguments)
#              490 POP_TOP
# 
# 23114:         492 LOAD_FAST            (self)
#              494 LOAD_ATTR            (ARGB_FAN_all)
#              496 LOAD_METHOD          (hide)
#              498 CALL_METHOD          (0 positional arguments)
#              500 POP_TOP
# 
# 23115:         502 LOAD_FAST            (self)
#              504 LOAD_ATTR            (chuji_effect_widget)
#              506 LOAD_METHOD          (hide)
#              508 CALL_METHOD          (0 positional arguments)
#              510 POP_TOP
# 
# 23117:         512 LOAD_FAST            (self)
#              514 LOAD_ATTR            (fan_lable_tese)
#              516 GET_ITER
#          >>  518 FOR_ITER             (to 534)
#              520 STORE_FAST           (i)
# 
# 23118:         522 LOAD_FAST            (i)
#              524 LOAD_METHOD          (hide)
#              526 CALL_METHOD          (0 positional arguments)
#              528 POP_TOP
#              530 EXTENDED_ARG         (512)
#              532 JUMP_ABSOLUTE        (to 518)
# 
# 23119:     >>  534 LOAD_FAST            (self)
#              536 LOAD_ATTR            (overlay)
#              538 LOAD_METHOD          (hide)
#              540 CALL_METHOD          (0 positional arguments)
#              542 POP_TOP
# 
# 23120:         544 LOAD_FAST            (self)
#              546 LOAD_ATTR            (top_zhanwei_widget)
#              548 LOAD_METHOD          (show)
#              550 CALL_METHOD          (0 positional arguments)
#              552 POP_TOP
# 
# 23122:     >>  554 LOAD_GLOBAL          (port)
#              556 LOAD_ATTR            (fan_argb_button_model_index)
#              558 LOAD_CONST           (1)
#              560 COMPARE_OP           (==)
#              562 EXTENDED_ARG         (1024)
#              564 POP_JUMP_IF_FALSE    (to 1090)
# 
# 23123:         566 LOAD_GLOBAL          (print)
#              568 LOAD_CONST           ("ARGB控制器选项卡")
#              570 CALL_FUNCTION        (1 positional argument)
#              572 POP_TOP
# 
# 23124:         574 LOAD_GLOBAL          (_last_future)
#              576 LOAD_CONST           (None)
#              578 COMPARE_OP           (is-not)
#              580 EXTENDED_ARG         (512)
#              582 POP_JUMP_IF_FALSE    (to 626)
# 
# 23125:         584 SETUP_FINALLY        (to 614)
# 
# 23126:     >>  586 LOAD_GLOBAL          (_last_future)
#              588 LOAD_METHOD          (done)
#              590 CALL_METHOD          (0 positional arguments)
#              592 EXTENDED_ARG         (512)
#              594 POP_JUMP_IF_TRUE     (to 610)
# 
# 23127:         596 LOAD_GLOBAL          (time)
#              598 LOAD_METHOD          (sleep)
#              600 LOAD_CONST           (0.01)
#              602 CALL_METHOD          (1 positional argument)
#              604 POP_TOP
#              606 EXTENDED_ARG         (512)
#              608 JUMP_ABSOLUTE        (to 586)
#          >>  610 POP_BLOCK
#              612 JUMP_FORWARD         (to 626)
# 
# 23128:     >>  614 POP_TOP
#              616 POP_TOP
#              618 POP_TOP
# 
# 23129:         620 POP_EXCEPT
#              622 JUMP_FORWARD         (to 626)
#              624 END_FINALLY
# 
# 23130:     >>  626 LOAD_FAST            (self)
#              628 LOAD_ATTR            (fan_config_dan_widget)
#              630 LOAD_METHOD          (show)
#              632 CALL_METHOD          (0 positional arguments)
#              634 POP_TOP
# 
# 23131:         636 LOAD_FAST            (self)
#              638 LOAD_ATTR            (system_info_widget)
#              640 LOAD_METHOD          (hide)
#              642 CALL_METHOD          (0 positional arguments)
#              644 POP_TOP
# 
# 23132:         646 LOAD_FAST            (self)
#              648 LOAD_ATTR            (main_widget_mubu)
#              650 LOAD_METHOD          (hide)
#              652 CALL_METHOD          (0 positional arguments)
#              654 POP_TOP
# 
# 23133:         656 LOAD_FAST            (self)
#              658 LOAD_ATTR            (effect_list_label)
#              660 LOAD_METHOD          (hide)
#              662 CALL_METHOD          (0 positional arguments)
#              664 POP_TOP
# 
# 23134:         666 LOAD_FAST            (self)
#              668 LOAD_ATTR            (fan_effect_list)
#              670 LOAD_METHOD          (hide)
#              672 CALL_METHOD          (0 positional arguments)
#              674 POP_TOP
# 
# 23135:         676 LOAD_FAST            (self)
#              678 LOAD_ATTR            (ARGB_FAN_sync_all)
#              680 LOAD_METHOD          (hide)
#              682 CALL_METHOD          (0 positional arguments)
#              684 POP_TOP
# 
# 23136:         686 LOAD_FAST            (self)
#              688 LOAD_ATTR            (ARGB_FAN_all)
#              690 LOAD_METHOD          (hide)
#              692 CALL_METHOD          (0 positional arguments)
#              694 POP_TOP
# 
# 23137:         696 LOAD_FAST            (self)
#              698 LOAD_ATTR            (chuji_effect_widget)
#              700 LOAD_METHOD          (hide)
#              702 CALL_METHOD          (0 positional arguments)
#              704 POP_TOP
# 
# 23139:         706 LOAD_FAST            (self)
#              708 LOAD_ATTR            (fan_lable_tese)
#              710 GET_ITER
#          >>  712 FOR_ITER             (to 728)
#              714 STORE_FAST           (i)
# 
# 23140:         716 LOAD_FAST            (i)
#              718 LOAD_METHOD          (hide)
#              720 CALL_METHOD          (0 positional arguments)
#              722 POP_TOP
#              724 EXTENDED_ARG         (512)
#              726 JUMP_ABSOLUTE        (to 712)
# 
# 23141:     >>  728 LOAD_FAST            (self)
#              730 LOAD_ATTR            (tongbu_effect_widget)
#              732 LOAD_METHOD          (show)
#              734 CALL_METHOD          (0 positional arguments)
#              736 POP_TOP
# 
# 23142:         738 LOAD_FAST            (self)
#              740 LOAD_ATTR            (effect_fan_speed_widget)
#              742 LOAD_METHOD          (show)
#              744 CALL_METHOD          (0 positional arguments)
#              746 POP_TOP
# 
# 23143:         748 LOAD_FAST            (self)
#              750 LOAD_ATTR            (tongbu_dengguang_on_off)
#              752 LOAD_METHOD          (show)
#              754 CALL_METHOD          (0 positional arguments)
#              756 POP_TOP
# 
# 23144:         758 LOAD_FAST            (self)
#              760 LOAD_ATTR            (tongbu_dengguang_on_off_switch)
#              762 LOAD_METHOD          (show)
#              764 CALL_METHOD          (0 positional arguments)
#              766 POP_TOP
# 
# 23145:         768 LOAD_FAST            (self)
#              770 LOAD_ATTR            (tongbu_dengguang_on_off_lable)
#              772 LOAD_METHOD          (show)
#              774 CALL_METHOD          (0 positional arguments)
#              776 POP_TOP
# 
# 23146:         778 LOAD_GLOBAL          (port)
#              780 LOAD_ATTR            (fan_advanced_effect)
#              782 LOAD_CONST           (0)
#              784 BINARY_SUBSCR
#              786 STORE_FAST           (vaule_argb)
# 
# 23148:         788 LOAD_FAST            (self)
#              790 LOAD_ATTR            (kaiguandeng_zhanwei)
#              792 LOAD_METHOD          (show)
#              794 CALL_METHOD          (0 positional arguments)
#              796 POP_TOP
# 
# 23149:         798 LOAD_FAST            (self)
#              800 LOAD_ATTR            (kaiguandeng_switch)
#              802 LOAD_METHOD          (show)
#              804 CALL_METHOD          (0 positional arguments)
#              806 POP_TOP
# 
# 23150:         808 LOAD_FAST            (self)
#              810 LOAD_ATTR            (kaiguandeng_lable_txt)
#              812 LOAD_METHOD          (show)
#              814 CALL_METHOD          (0 positional arguments)
#              816 POP_TOP
# 
# 23151:         818 LOAD_GLOBAL          (port)
#              820 LOAD_ATTR            (fan_advanced_effect)
#              822 LOAD_CONST           (13)
#              824 BINARY_SUBSCR
#              826 LOAD_CONST           (0)
#              828 COMPARE_OP           (==)
#              830 EXTENDED_ARG         (768)
#              832 POP_JUMP_IF_FALSE    (to 844)
# 
# 23152:         834 LOAD_FAST            (self)
#              836 LOAD_ATTR            (kaiguandeng_shuxian)
#              838 LOAD_METHOD          (show)
#              840 CALL_METHOD          (0 positional arguments)
#              842 POP_TOP
# 
# 23154:     >>  844 LOAD_GLOBAL          (port)
#              846 LOAD_ATTR            (MCU_C_FLAG)
#              848 LOAD_CONST           (0)
#              850 BINARY_SUBSCR
#              852 LOAD_CONST           (1)
#              854 COMPARE_OP           (==)
#              856 EXTENDED_ARG         (768)
#              858 POP_JUMP_IF_FALSE    (to 874)
# 
# 23155:         860 LOAD_FAST            (self)
#              862 LOAD_ATTR            (tongbu_dengguang_on_off_switch)
#              864 LOAD_METHOD          (setChecked)
#              866 LOAD_CONST           (True)
#              868 CALL_METHOD          (1 positional argument)
#              870 POP_TOP
#              872 JUMP_FORWARD         (to 886)
# 
# 23158:     >>  874 LOAD_FAST            (self)
#              876 LOAD_ATTR            (tongbu_dengguang_on_off_switch)
#              878 LOAD_METHOD          (setChecked)
#              880 LOAD_CONST           (False)
#              882 CALL_METHOD          (1 positional argument)
#              884 POP_TOP
# 
# 23160:     >>  886 LOAD_GLOBAL          (port)
#              888 LOAD_ATTR            (MCU_C_FLAG)
#              890 LOAD_CONST           (1)
#              892 BINARY_SUBSCR
#              894 LOAD_CONST           (1)
#              896 COMPARE_OP           (==)
#              898 EXTENDED_ARG         (768)
#              900 POP_JUMP_IF_FALSE    (to 924)
#              902 LOAD_FAST            (vaule_argb)
#              904 LOAD_CONST           (3)
#              906 COMPARE_OP           (==)
#              908 EXTENDED_ARG         (768)
#              910 POP_JUMP_IF_FALSE    (to 924)
# 
# 23161:         912 LOAD_FAST            (self)
#              914 LOAD_ATTR            (tongbu_speed_on_off_switch)
#              916 LOAD_METHOD          (setChecked)
#              918 LOAD_CONST           (True)
#              920 CALL_METHOD          (1 positional argument)
#              922 POP_TOP
# 
# 23163:     >>  924 LOAD_FAST            (vaule_argb)
#              926 LOAD_CONST           (3)
#              928 COMPARE_OP           (<)
#              930 EXTENDED_ARG         (768)
#              932 POP_JUMP_IF_TRUE     (to 944)
#              934 LOAD_FAST            (vaule_argb)
#              936 LOAD_CONST           (4)
#              938 COMPARE_OP           (==)
#              940 EXTENDED_ARG         (768)
#              942 POP_JUMP_IF_FALSE    (to 992)
# 
# 23164:     >>  944 LOAD_GLOBAL          (port)
#              946 LOAD_ATTR            (MCU_C_FLAG)
#              948 LOAD_CONST           (0)
#              950 BINARY_SUBSCR
#              952 LOAD_CONST           (1)
#              954 COMPARE_OP           (==)
#              956 EXTENDED_ARG         (768)
#              958 POP_JUMP_IF_FALSE    (to 982)
# 
# 23165:         960 LOAD_FAST            (self)
#              962 LOAD_ATTR            (overlay)
#              964 LOAD_METHOD          (show)
#              966 CALL_METHOD          (0 positional arguments)
#              968 POP_TOP
# 
# 23166:         970 LOAD_FAST            (self)
#              972 LOAD_ATTR            (overlay)
#              974 LOAD_METHOD          (raise_)
#              976 CALL_METHOD          (0 positional arguments)
#              978 POP_TOP
#              980 JUMP_FORWARD         (to 992)
# 
# 23168:     >>  982 LOAD_FAST            (self)
#              984 LOAD_ATTR            (overlay)
#              986 LOAD_METHOD          (hide)
#              988 CALL_METHOD          (0 positional arguments)
#              990 POP_TOP
# 
# 23169:     >>  992 LOAD_FAST            (vaule_argb)
#              994 LOAD_CONST           (3)
#              996 COMPARE_OP           (==)
#              998 EXTENDED_ARG         (1024)
#             1000 POP_JUMP_IF_FALSE    (to 1050)
# 
# 23170:        1002 LOAD_GLOBAL          (port)
#             1004 LOAD_ATTR            (MCU_C_FLAG)
#             1006 LOAD_CONST           (1)
#             1008 BINARY_SUBSCR
#             1010 LOAD_CONST           (1)
#             1012 COMPARE_OP           (==)
#             1014 EXTENDED_ARG         (1024)
#             1016 POP_JUMP_IF_FALSE    (to 1040)
# 
# 23171:        1018 LOAD_FAST            (self)
#             1020 LOAD_ATTR            (overlay)
#             1022 LOAD_METHOD          (show)
#             1024 CALL_METHOD          (0 positional arguments)
#             1026 POP_TOP
# 
# 23172:        1028 LOAD_FAST            (self)
#             1030 LOAD_ATTR            (overlay)
#             1032 LOAD_METHOD          (raise_)
#             1034 CALL_METHOD          (0 positional arguments)
#             1036 POP_TOP
#             1038 JUMP_FORWARD         (to 1050)
# 
# 23174:     >> 1040 LOAD_FAST            (self)
#             1042 LOAD_ATTR            (overlay)
#             1044 LOAD_METHOD          (hide)
#             1046 CALL_METHOD          (0 positional arguments)
#             1048 POP_TOP
# 
# 23175:     >> 1050 LOAD_GLOBAL          (port)
#             1052 LOAD_ATTR            (fan_advanced_effect)
#             1054 LOAD_CONST           (1)
#             1056 BINARY_SUBSCR
#             1058 LOAD_CONST           (1)
#             1060 COMPARE_OP           (==)
#             1062 EXTENDED_ARG         (1024)
#             1064 POP_JUMP_IF_FALSE    (to 1076)
# 
# 23176:        1066 LOAD_CONST           (0)
#             1068 LOAD_GLOBAL          (port)
#             1070 LOAD_ATTR            (fan_advanced_effect)
#             1072 LOAD_CONST           (1)
#             1074 STORE_SUBSCR
# 
# 23177:     >> 1076 LOAD_FAST            (self)
#             1078 LOAD_ATTR            (fan_effect_speed_buttons)
#             1080 LOAD_FAST            (vaule_argb)
#             1082 BINARY_SUBSCR
#             1084 LOAD_METHOD          (click)
#             1086 CALL_METHOD          (0 positional arguments)
#             1088 POP_TOP
# 
# 23179:     >> 1090 LOAD_GLOBAL          (port)
#             1092 LOAD_ATTR            (fan_argb_button_model_index)
#             1094 LOAD_CONST           (2)
#             1096 COMPARE_OP           (==)
#             1098 EXTENDED_ARG         (1280)
#             1100 POP_JUMP_IF_FALSE    (to 1374)
# 
# 23180:        1102 LOAD_FAST            (self)
#             1104 LOAD_ATTR            (kaiguandeng_switch)
#             1106 LOAD_METHOD          (show)
#             1108 CALL_METHOD          (0 positional arguments)
#             1110 POP_TOP
# 
# 23181:        1112 LOAD_FAST            (self)
#             1114 LOAD_ATTR            (kaiguandeng_lable_txt)
#             1116 LOAD_METHOD          (show)
#             1118 CALL_METHOD          (0 positional arguments)
#             1120 POP_TOP
# 
# 23182:        1122 LOAD_FAST            (self)
#             1124 LOAD_ATTR            (top_zhanwei_widget)
#             1126 LOAD_METHOD          (show)
#             1128 CALL_METHOD          (0 positional arguments)
#             1130 POP_TOP
# 
# 23183:        1132 LOAD_FAST            (self)
#             1134 LOAD_ATTR            (kaiguandeng_zhanwei)
#             1136 LOAD_METHOD          (show)
#             1138 CALL_METHOD          (0 positional arguments)
#             1140 POP_TOP
# 
# 23184:        1142 LOAD_GLOBAL          (agreement)
#             1144 LOAD_METHOD          (ARGB_MCU_C_changer)
#             1146 LOAD_GLOBAL          (port)
#             1148 LOAD_ATTR            (ARGB_ser)
#             1150 LOAD_CONST           (1)
#             1152 CALL_METHOD          (2 positional arguments)
#             1154 POP_TOP
# 
# 23185:        1156 LOAD_GLOBAL          (port)
#             1158 LOAD_ATTR            (fan_advanced_effect)
#             1160 LOAD_CONST           (13)
#             1162 BINARY_SUBSCR
#             1164 LOAD_CONST           (1)
#             1166 COMPARE_OP           (==)
#             1168 EXTENDED_ARG         (1024)
#             1170 POP_JUMP_IF_FALSE    (to 1184)
# 
# 23186:        1172 LOAD_GLOBAL          (agreement)
#             1174 LOAD_METHOD          (argb_fan_light_off)
#             1176 LOAD_GLOBAL          (port)
#             1178 LOAD_ATTR            (ARGB_ser)
#             1180 CALL_METHOD          (1 positional argument)
#             1182 POP_TOP
# 
# 23187:     >> 1184 LOAD_GLOBAL          (sum)
#             1186 LOAD_GLOBAL          (port)
#             1188 LOAD_ATTR            (ARGB_TEN_NUM)
#             1190 CALL_FUNCTION        (1 positional argument)
#             1192 STORE_FAST           (sum_rgb)
# 
# 23188:        1194 LOAD_CONST           (160)
#             1196 STORE_FAST           (C_NUM)
# 
# 23189:        1198 LOAD_FAST            (sum_rgb)
#             1200 LOAD_FAST            (C_NUM)
#             1202 BINARY_FLOOR_DIVIDE
#             1204 STORE_FAST           (sum_send_z)
# 
# 23190:        1206 LOAD_FAST            (sum_rgb)
#             1208 LOAD_FAST            (C_NUM)
#             1210 BINARY_MODULO
#             1212 STORE_FAST           (sum_send_y)
# 
# 23191:        1214 LOAD_FAST            (sum_send_y)
#             1216 EXTENDED_ARG         (1024)
#             1218 POP_JUMP_IF_FALSE    (to 1228)
# 
# 23192:        1220 LOAD_FAST            (sum_send_z)
#             1222 LOAD_CONST           (1)
#             1224 INPLACE_ADD
#             1226 STORE_FAST           (sum_send_z)
# 
# 23193:     >> 1228 LOAD_CONST           (1)
#             1230 STORE_FAST           (sum_send_z)
# 
# 23194:        1232 LOAD_GLOBAL          (print)
#             1234 LOAD_CONST           ("间隔次数：")
#             1236 LOAD_FAST            (sum_send_z)
#             1238 FORMAT_VALUE         0
#             1240 LOAD_CONST           (", 灯珠总数：")
#             1242 LOAD_FAST            (sum_rgb)
#             1244 FORMAT_VALUE         0
#             1246 BUILD_STRING         4
#             1248 CALL_FUNCTION        (1 positional argument)
#             1250 POP_TOP
# 
# 23195:        1252 LOAD_FAST            (sum_send_z)
#             1254 LOAD_FAST            (self)
#             1256 LOAD_ATTR            (canvas)
#             1258 LOAD_ATTR            (scene)
#             1260 STORE_ATTR           (send_flag_devices_fan)
# 
# 23196:        1262 LOAD_GLOBAL          (print)
#             1264 LOAD_CONST           ("幕布灯效选项卡")
#             1266 CALL_FUNCTION        (1 positional argument)
#             1268 POP_TOP
# 
# 23198:        1270 LOAD_FAST            (self)
#             1272 LOAD_ATTR            (chuji_effect_widget)
#             1274 LOAD_METHOD          (hide)
#             1276 CALL_METHOD          (0 positional arguments)
#             1278 POP_TOP
# 
# 23199:        1280 LOAD_FAST            (self)
#             1282 LOAD_ATTR            (main_widget_mubu)
#             1284 LOAD_METHOD          (show)
#             1286 CALL_METHOD          (0 positional arguments)
#             1288 POP_TOP
# 
# 23201:        1290 LOAD_FAST            (self)
#             1292 LOAD_ATTR            (canvas)
#             1294 LOAD_ATTR            (scene)
#             1296 LOAD_METHOD          (load_icons_state)
#             1298 CALL_METHOD          (0 positional arguments)
#             1300 POP_TOP
# 
# 23202:        1302 LOAD_FAST            (self)
#             1304 LOAD_ATTR            (canvas)
#             1306 LOAD_ATTR            (scene)
#             1308 LOAD_METHOD          (update)
#             1310 CALL_METHOD          (0 positional arguments)
#             1312 POP_TOP
# 
# 23203:        1314 LOAD_FAST            (self)
#             1316 LOAD_METHOD          (on_effect_changed)
#             1318 LOAD_FAST            (self)
#             1320 LOAD_ATTR            (effect_manager)
#             1322 LOAD_ATTR            (current_effect)
#             1324 CALL_METHOD          (1 positional argument)
#             1326 POP_TOP
# 
# 23204:        1328 LOAD_FAST            (self)
#             1330 LOAD_ATTR            (overlay)
#             1332 LOAD_METHOD          (hide)
#             1334 CALL_METHOD          (0 positional arguments)
#             1336 POP_TOP
# 
# 23205:        1338 LOAD_GLOBAL          (port)
#             1340 LOAD_ATTR            (fan_advanced_effect)
#             1342 LOAD_CONST           (13)
#             1344 BINARY_SUBSCR
#             1346 LOAD_CONST           (1)
#             1348 COMPARE_OP           (==)
#             1350 EXTENDED_ARG         (1280)
#             1352 POP_JUMP_IF_FALSE    (to 1374)
# 
# 23206:        1354 LOAD_FAST            (self)
#             1356 LOAD_ATTR            (overlay)
#             1358 LOAD_METHOD          (show)
#             1360 CALL_METHOD          (0 positional arguments)
#             1362 POP_TOP
# 
# 23207:        1364 LOAD_FAST            (self)
#             1366 LOAD_ATTR            (overlay)
#             1368 LOAD_METHOD          (raise_)
#             1370 CALL_METHOD          (0 positional arguments)
#             1372 POP_TOP
# 
# 23208:     >> 1374 LOAD_GLOBAL          (port)
#             1376 LOAD_ATTR            (fan_argb_button_model_index)
#             1378 LOAD_CONST           (3)
#             1380 COMPARE_OP           (==)
#             1382 EXTENDED_ARG         (1280)
#             1384 POP_JUMP_IF_FALSE    (to 1520)
# 
# 23209:        1386 LOAD_GLOBAL          (print)
#             1388 LOAD_CONST           ("设置选项卡")
#             1390 CALL_FUNCTION        (1 positional argument)
#             1392 POP_TOP
# 
# 23210:        1394 LOAD_FAST            (self)
#             1396 LOAD_ATTR            (main_widget_mubu)
#             1398 LOAD_METHOD          (hide)
#             1400 CALL_METHOD          (0 positional arguments)
#             1402 POP_TOP
# 
# 23211:        1404 LOAD_FAST            (self)
#             1406 LOAD_ATTR            (effect_list_label)
#             1408 LOAD_METHOD          (hide)
#             1410 CALL_METHOD          (0 positional arguments)
#             1412 POP_TOP
# 
# 23212:        1414 LOAD_FAST            (self)
#             1416 LOAD_ATTR            (fan_effect_list)
#             1418 LOAD_METHOD          (hide)
#             1420 CALL_METHOD          (0 positional arguments)
#             1422 POP_TOP
# 
# 23213:        1424 LOAD_FAST            (self)
#             1426 LOAD_ATTR            (ARGB_FAN_sync_all)
#             1428 LOAD_METHOD          (hide)
#             1430 CALL_METHOD          (0 positional arguments)
#             1432 POP_TOP
# 
# 23214:        1434 LOAD_FAST            (self)
#             1436 LOAD_ATTR            (ARGB_FAN_all)
#             1438 LOAD_METHOD          (hide)
#             1440 CALL_METHOD          (0 positional arguments)
#             1442 POP_TOP
# 
# 23215:        1444 LOAD_FAST            (self)
#             1446 LOAD_ATTR            (chuji_effect_widget)
#             1448 LOAD_METHOD          (hide)
#             1450 CALL_METHOD          (0 positional arguments)
#             1452 POP_TOP
# 
# 23216:        1454 LOAD_FAST            (self)
#             1456 LOAD_ATTR            (fan_lable_tese)
#             1458 GET_ITER
#          >> 1460 FOR_ITER             (to 1476)
#             1462 STORE_FAST           (i)
# 
# 23217:        1464 LOAD_FAST            (i)
#             1466 LOAD_METHOD          (hide)
#             1468 CALL_METHOD          (0 positional arguments)
#             1470 POP_TOP
#             1472 EXTENDED_ARG         (1280)
#             1474 JUMP_ABSOLUTE        (to 1460)
# 
# 23218:     >> 1476 LOAD_FAST            (self)
#             1478 LOAD_ATTR            (new_shezhi_widget)
#             1480 LOAD_METHOD          (show)
#             1482 CALL_METHOD          (0 positional arguments)
#             1484 POP_TOP
# 
# 23219:        1486 LOAD_FAST            (self)
#             1488 LOAD_ATTR            (shezhi_tongyong_widget)
#             1490 LOAD_METHOD          (show)
#             1492 CALL_METHOD          (0 positional arguments)
#             1494 POP_TOP
# 
# 23220:        1496 LOAD_FAST            (self)
#             1498 LOAD_ATTR            (overlay)
#             1500 LOAD_METHOD          (hide)
#             1502 CALL_METHOD          (0 positional arguments)
#             1504 POP_TOP
# 
# 23221:        1506 LOAD_FAST            (self)
#             1508 LOAD_ATTR            (new_shezhi_buttons)
#             1510 LOAD_CONST           (0)
#             1512 BINARY_SUBSCR
#             1514 LOAD_METHOD          (click)
#             1516 CALL_METHOD          (0 positional arguments)
#             1518 POP_TOP
# 
# 23222:     >> 1520 LOAD_GLOBAL          (port)
#             1522 LOAD_ATTR            (fan_argb_button_model_index)
#             1524 LOAD_CONST           (5)
#             1526 COMPARE_OP           (==)
#             1528 EXTENDED_ARG         (3584)
#             1530 POP_JUMP_IF_FALSE    (to 3634)
# 
# 23223:        1532 LOAD_FAST            (self)
#             1534 LOAD_ATTR            (main_widget_mubu)
#             1536 LOAD_METHOD          (hide)
#             1538 CALL_METHOD          (0 positional arguments)
#             1540 POP_TOP
# 
# 23226:        1542 LOAD_GLOBAL          (port)
#             1544 LOAD_ATTR            (fan_advanced_effect)
#             1546 LOAD_CONST           (0)
#             1548 BINARY_SUBSCR
#             1550 STORE_FAST           (effect)
# 
# 23227:        1552 LOAD_GLOBAL          (port)
#             1554 LOAD_ATTR            (fan_advanced_effect)
#             1556 LOAD_CONST           (1)
#             1558 BINARY_SUBSCR
#             1560 STORE_FAST           (fan_index)
# 
# 23228:        1562 LOAD_GLOBAL          (port)
#             1564 LOAD_ATTR            (fan_advanced_effect)
#             1566 LOAD_CONST           (2)
#             1568 BINARY_SUBSCR
#             1570 STORE_FAST           (async_index)
# 
# 23229:        1572 LOAD_GLOBAL          (port)
#             1574 LOAD_ATTR            (fan_advanced_effect)
#             1576 LOAD_CONST           (3)
#             1578 BINARY_SUBSCR
#             1580 STORE_FAST           (tongp_shiy_index)
# 
# 23230:        1582 LOAD_GLOBAL          (print)
#             1584 LOAD_CONST           ("高级灯效选项卡 ")
#             1586 LOAD_GLOBAL          (port)
#             1588 LOAD_ATTR            (fan_advanced_effect)
#             1590 FORMAT_VALUE         0
#             1592 BUILD_STRING         2
#             1594 CALL_FUNCTION        (1 positional argument)
#             1596 POP_TOP
# 
# 23244:        1598 LOAD_FAST            (self)
#             1600 LOAD_ATTR            (liusu_fan_lable)
#             1602 LOAD_METHOD          (show)
#             1604 CALL_METHOD          (0 positional arguments)
#             1606 POP_TOP
# 
# 23245:        1608 LOAD_FAST            (self)
#             1610 LOAD_ATTR            (slider_fan_liusu)
#             1612 LOAD_METHOD          (show)
#             1614 CALL_METHOD          (0 positional arguments)
#             1616 POP_TOP
# 
# 23246:        1618 LOAD_FAST            (self)
#             1620 LOAD_ATTR            (liusu_fan_text_lable)
#             1622 LOAD_METHOD          (show)
#             1624 CALL_METHOD          (0 positional arguments)
#             1626 POP_TOP
# 
# 23248:        1628 LOAD_FAST            (self)
#             1630 LOAD_ATTR            (fangxiang_fan_lable)
#             1632 LOAD_METHOD          (show)
#             1634 CALL_METHOD          (0 positional arguments)
#             1636 POP_TOP
# 
# 23249:        1638 LOAD_FAST            (self)
#             1640 LOAD_ATTR            (fan_fangxiang_buttons)
#             1642 GET_ITER
#          >> 1644 FOR_ITER             (to 1660)
#             1646 STORE_FAST           (i)
# 
# 23250:        1648 LOAD_FAST            (i)
#             1650 LOAD_METHOD          (hide)
#             1652 CALL_METHOD          (0 positional arguments)
#             1654 POP_TOP
#             1656 EXTENDED_ARG         (1536)
#             1658 JUMP_ABSOLUTE        (to 1644)
# 
# 23252:     >> 1660 LOAD_FAST            (self)
#             1662 LOAD_ATTR            (liangdu_fan_lable)
#             1664 LOAD_METHOD          (show)
#             1666 CALL_METHOD          (0 positional arguments)
#             1668 POP_TOP
# 
# 23253:        1670 LOAD_FAST            (self)
#             1672 LOAD_ATTR            (slider_fan_liangdu)
#             1674 LOAD_METHOD          (show)
#             1676 CALL_METHOD          (0 positional arguments)
#             1678 POP_TOP
# 
# 23254:        1680 LOAD_FAST            (self)
#             1682 LOAD_ATTR            (liangdu_fan_text_lable)
#             1684 LOAD_METHOD          (show)
#             1686 CALL_METHOD          (0 positional arguments)
#             1688 POP_TOP
# 
# 23256:        1690 LOAD_FAST            (self)
#             1692 LOAD_ATTR            (yanse_fan_lable)
#             1694 LOAD_METHOD          (show)
#             1696 CALL_METHOD          (0 positional arguments)
#             1698 POP_TOP
# 
# 23257:        1700 LOAD_FAST            (self)
#             1702 LOAD_ATTR            (fan_dengxiao_buttons)
#             1704 GET_ITER
#          >> 1706 FOR_ITER             (to 1722)
#             1708 STORE_FAST           (i)
# 
# 23258:        1710 LOAD_FAST            (i)
#             1712 LOAD_METHOD          (show)
#             1714 CALL_METHOD          (0 positional arguments)
#             1716 POP_TOP
#             1718 EXTENDED_ARG         (1536)
#             1720 JUMP_ABSOLUTE        (to 1706)
# 
# 23259:     >> 1722 LOAD_FAST            (self)
#             1724 LOAD_ATTR            (button_fan_tiaose)
#             1726 LOAD_METHOD          (show)
#             1728 CALL_METHOD          (0 positional arguments)
#             1730 POP_TOP
# 
# 23261:        1732 LOAD_FAST            (self)
#             1734 LOAD_ATTR            (fan_effect_list)
#             1736 LOAD_METHOD          (blockSignals)
#             1738 LOAD_CONST           (True)
#             1740 CALL_METHOD          (1 positional argument)
#             1742 POP_TOP
# 
# 23262:        1744 LOAD_FAST            (self)
#             1746 LOAD_ATTR            (fan_effect_list)
#             1748 LOAD_METHOD          (clear)
#             1750 CALL_METHOD          (0 positional arguments)
#             1752 POP_TOP
# 
# 23263:        1754 LOAD_GLOBAL          (port)
#             1756 LOAD_ATTR            (language)
#             1758 LOAD_CONST           ("zh")
#             1760 COMPARE_OP           (==)
#             1762 EXTENDED_ARG         (1536)
#             1764 POP_JUMP_IF_FALSE    (to 1774)
# 
# 23264:        1766 LOAD_GLOBAL          (port)
#             1768 LOAD_ATTR            (dengxiao_model2_zh)
#             1770 STORE_FAST           (dengxiao)
#             1772 JUMP_FORWARD         (to 1780)
# 
# 23266:     >> 1774 LOAD_GLOBAL          (port)
#             1776 LOAD_ATTR            (dengxiao_model2_en)
#             1778 STORE_FAST           (dengxiao)
# 
# 23267:     >> 1780 LOAD_FAST            (self)
#             1782 LOAD_ATTR            (fan_effect_list)
#             1784 LOAD_METHOD          (addItems)
#             1786 LOAD_FAST            (dengxiao)
#             1788 CALL_METHOD          (1 positional argument)
#             1790 POP_TOP
# 
# 23268:        1792 LOAD_FAST            (self)
#             1794 LOAD_ATTR            (fan_effect_list)
#             1796 LOAD_METHOD          (setCurrentIndex)
#             1798 LOAD_FAST            (effect)
#             1800 CALL_METHOD          (1 positional argument)
#             1802 POP_TOP
# 
# 23269:        1804 LOAD_FAST            (self)
#             1806 LOAD_ATTR            (fan_effect_list)
#             1808 LOAD_METHOD          (blockSignals)
#             1810 LOAD_CONST           (False)
#             1812 CALL_METHOD          (1 positional argument)
#             1814 POP_TOP
# 
# 23270:        1816 LOAD_FAST            (self)
#             1818 LOAD_ATTR            (effect_list_label)
#             1820 LOAD_METHOD          (show)
#             1822 CALL_METHOD          (0 positional arguments)
#             1824 POP_TOP
# 
# 23271:        1826 LOAD_FAST            (self)
#             1828 LOAD_ATTR            (fan_effect_list)
#             1830 LOAD_METHOD          (show)
#             1832 CALL_METHOD          (0 positional arguments)
#             1834 POP_TOP
# 
# 23272:        1836 LOAD_FAST            (self)
#             1838 LOAD_ATTR            (chuji_effect_widget)
#             1840 LOAD_METHOD          (hide)
#             1842 CALL_METHOD          (0 positional arguments)
#             1844 POP_TOP
# 
# 23273:        1846 LOAD_GLOBAL          (range)
#             1848 LOAD_CONST           (10)
#             1850 CALL_FUNCTION        (1 positional argument)
#             1852 GET_ITER
#          >> 1854 FOR_ITER             (to 1938)
#             1856 STORE_FAST           (i)
# 
# 23274:        1858 LOAD_GLOBAL          (port)
#             1860 LOAD_ATTR            (ARGB_TEN_NUM)
#             1862 LOAD_FAST            (i)
#             1864 BINARY_SUBSCR
#             1866 LOAD_CONST           (0)
#             1868 COMPARE_OP           (==)
#             1870 EXTENDED_ARG         (1792)
#             1872 POP_JUMP_IF_FALSE    (to 1890)
# 
# 23275:        1874 LOAD_FAST            (self)
#             1876 LOAD_ATTR            (ARGB_FAN_devices)
#             1878 LOAD_FAST            (i)
#             1880 BINARY_SUBSCR
#             1882 LOAD_METHOD          (hide)
#             1884 CALL_METHOD          (0 positional arguments)
#             1886 POP_TOP
#             1888 JUMP_FORWARD         (to 1904)
# 
# 23277:     >> 1890 LOAD_FAST            (self)
#             1892 LOAD_ATTR            (ARGB_FAN_devices)
#             1894 LOAD_FAST            (i)
#             1896 BINARY_SUBSCR
#             1898 LOAD_METHOD          (show)
#             1900 CALL_METHOD          (0 positional arguments)
#             1902 POP_TOP
# 
# 23278:     >> 1904 LOAD_GLOBAL          (port)
#             1906 LOAD_ATTR            (ARGB_TEN_NUM)
#             1908 LOAD_CONST           (0)
#             1910 BINARY_SUBSCR
#             1912 LOAD_CONST           (6)
#             1914 COMPARE_OP           (==)
#             1916 EXTENDED_ARG         (1792)
#             1918 POP_JUMP_IF_FALSE    (to 1854)
# 
# 23279:        1920 LOAD_FAST            (self)
#             1922 LOAD_ATTR            (ARGB_FAN_devices)
#             1924 LOAD_CONST           (0)
#             1926 BINARY_SUBSCR
#             1928 LOAD_METHOD          (hide)
#             1930 CALL_METHOD          (0 positional arguments)
#             1932 POP_TOP
#             1934 EXTENDED_ARG         (1792)
#             1936 JUMP_ABSOLUTE        (to 1854)
# 
# 23280:     >> 1938 LOAD_GLOBAL          (range)
#             1940 LOAD_CONST           (6)
#             1942 CALL_FUNCTION        (1 positional argument)
#             1944 GET_ITER
#          >> 1946 FOR_ITER             (to 1968)
#             1948 STORE_FAST           (i)
# 
# 23281:        1950 LOAD_FAST            (self)
#             1952 LOAD_ATTR            (ARGB_FAN_chuan)
#             1954 LOAD_FAST            (i)
#             1956 BINARY_SUBSCR
#             1958 LOAD_METHOD          (hide)
#             1960 CALL_METHOD          (0 positional arguments)
#             1962 POP_TOP
#             1964 EXTENDED_ARG         (1792)
#             1966 JUMP_ABSOLUTE        (to 1946)
# 
# 23282:     >> 1968 LOAD_FAST            (self)
#             1970 LOAD_ATTR            (fan_lable_tese)
#             1972 GET_ITER
#          >> 1974 FOR_ITER             (to 1990)
#             1976 STORE_FAST           (i)
# 
# 23283:        1978 LOAD_FAST            (i)
#             1980 LOAD_METHOD          (show)
#             1982 CALL_METHOD          (0 positional arguments)
#             1984 POP_TOP
#             1986 EXTENDED_ARG         (1792)
#             1988 JUMP_ABSOLUTE        (to 1974)
# 
# 23284:     >> 1990 LOAD_FAST            (self)
#             1992 LOAD_METHOD          (tese_tongping_shiyin_xianshi)
#             1994 LOAD_CONST           (4)
#             1996 CALL_METHOD          (1 positional argument)
#             1998 POP_TOP
# 
# 23285:        2000 LOAD_FAST            (self)
#             2002 LOAD_METHOD          (fan_effect_settings)
#             2004 LOAD_FAST            (effect)
#             2006 CALL_METHOD          (1 positional argument)
#             2008 POP_TOP
# 
# 23286:        2010 LOAD_FAST            (self)
#             2012 LOAD_ATTR            (anyc_list)
#             2014 LOAD_FAST            (effect)
#             2016 BINARY_SUBSCR
#             2018 LOAD_CONST           (255)
#             2020 COMPARE_OP           (!=)
#             2022 EXTENDED_ARG         (2048)
#             2024 POP_JUMP_IF_FALSE    (to 2048)
# 
# 23287:        2026 LOAD_FAST            (self)
#             2028 LOAD_ATTR            (ARGB_FAN_all)
#             2030 LOAD_METHOD          (show)
#             2032 CALL_METHOD          (0 positional arguments)
#             2034 POP_TOP
# 
# 23288:        2036 LOAD_FAST            (self)
#             2038 LOAD_ATTR            (ARGB_FAN_sync_all)
#             2040 LOAD_METHOD          (show)
#             2042 CALL_METHOD          (0 positional arguments)
#             2044 POP_TOP
#             2046 JUMP_FORWARD         (to 2068)
# 
# 23290:     >> 2048 LOAD_FAST            (self)
#             2050 LOAD_ATTR            (ARGB_FAN_all)
#             2052 LOAD_METHOD          (show)
#             2054 CALL_METHOD          (0 positional arguments)
#             2056 POP_TOP
# 
# 23291:        2058 LOAD_FAST            (self)
#             2060 LOAD_ATTR            (ARGB_FAN_sync_all)
#             2062 LOAD_METHOD          (hide)
#             2064 CALL_METHOD          (0 positional arguments)
#             2066 POP_TOP
# 
# 23292:     >> 2068 LOAD_FAST            (async_index)
#             2070 LOAD_CONST           (1)
#             2072 COMPARE_OP           (==)
#             2074 EXTENDED_ARG         (2048)
#             2076 POP_JUMP_IF_FALSE    (to 2290)
# 
# 23293:        2078 LOAD_GLOBAL          (print)
#             2080 LOAD_CONST           ("异步")
#             2082 CALL_FUNCTION        (1 positional argument)
#             2084 POP_TOP
# 
# 23294:        2086 LOAD_FAST            (self)
#             2088 LOAD_ATTR            (ARGB_FAN_all)
#             2090 LOAD_METHOD          (setStyleSheet)
# 
# 23295:        2092 LOAD_CONST           ("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
# 
# 23294:        2094 CALL_METHOD          (1 positional argument)
#             2096 POP_TOP
# 
# 23303:        2098 LOAD_FAST            (self)
#             2100 LOAD_ATTR            (ARGB_FAN_sync_all)
#             2102 LOAD_METHOD          (setStyleSheet)
# 
# 23304:        2104 LOAD_CONST           ("background-color: rgb(25,78,255);color: rgb(243,244,243);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
# 
# 23303:        2106 CALL_METHOD          (1 positional argument)
#             2108 POP_TOP
# 
# 23312:        2110 LOAD_GLOBAL          (range)
#             2112 LOAD_CONST           (10)
#             2114 CALL_FUNCTION        (1 positional argument)
#             2116 GET_ITER
#          >> 2118 FOR_ITER             (to 2180)
#             2120 STORE_FAST           (i)
# 
# 23313:        2122 LOAD_FAST            (self)
#             2124 LOAD_ATTR            (ARGB_FAN_devices)
#             2126 LOAD_FAST            (i)
#             2128 BINARY_SUBSCR
#             2130 LOAD_METHOD          (setIcon)
# 
# 23314:        2132 LOAD_GLOBAL          (QIcon)
#             2134 LOAD_GLOBAL          (port)
#             2136 LOAD_METHOD          (file_path_img)
#             2138 LOAD_GLOBAL          (port)
#             2140 LOAD_METHOD          (file_path_img)
#             2142 LOAD_FAST            (self)
#             2144 LOAD_ATTR            (ARGB_FAN_devices_img_xuanzhong)
#             2146 LOAD_FAST            (i)
#             2148 BINARY_SUBSCR
#             2150 CALL_METHOD          (1 positional argument)
#             2152 CALL_METHOD          (1 positional argument)
#             2154 CALL_FUNCTION        (1 positional argument)
# 
# 23313:        2156 CALL_METHOD          (1 positional argument)
#             2158 POP_TOP
# 
# 23315:        2160 LOAD_FAST            (self)
#             2162 LOAD_ATTR            (ARGB_FAN_devices)
#             2164 LOAD_FAST            (i)
#             2166 BINARY_SUBSCR
#             2168 LOAD_METHOD          (setChecked)
#             2170 LOAD_CONST           (False)
#             2172 CALL_METHOD          (1 positional argument)
#             2174 POP_TOP
#             2176 EXTENDED_ARG         (2048)
#             2178 JUMP_ABSOLUTE        (to 2118)
# 
# 23318:     >> 2180 LOAD_FAST            (self)
#             2182 LOAD_METHOD          (ARGB_send_show_effect)
#             2184 CALL_METHOD          (0 positional arguments)
#             2186 POP_TOP
# 
# 23319:        2188 LOAD_FAST            (self)
#             2190 LOAD_ATTR            (anyc_list)
#             2192 LOAD_FAST            (effect)
#             2194 BINARY_SUBSCR
#             2196 LOAD_GLOBAL          (agreement)
#             2198 LOAD_ATTR            (fan_order_list)
#             2200 LOAD_CONST           (0)
#             2202 STORE_SUBSCR
# 
# 23320:        2204 LOAD_GLOBAL          (range)
#             2206 LOAD_CONST           (15)
#             2208 CALL_FUNCTION        (1 positional argument)
#             2210 GET_ITER
#          >> 2212 FOR_ITER             (to 2244)
#             2214 STORE_FAST           (i)
# 
# 23321:        2216 LOAD_GLOBAL          (port)
#             2218 LOAD_ATTR            (ARGB_qun_async_config)
#             2220 LOAD_FAST            (effect)
#             2222 BINARY_SUBSCR
#             2224 LOAD_FAST            (i)
#             2226 BINARY_SUBSCR
#             2228 LOAD_GLOBAL          (agreement)
#             2230 LOAD_ATTR            (fan_order_list)
#             2232 LOAD_CONST           (1)
#             2234 LOAD_FAST            (i)
#             2236 BINARY_ADD
#             2238 STORE_SUBSCR
#             2240 EXTENDED_ARG         (2048)
#             2242 JUMP_ABSOLUTE        (to 2212)
# 
# 23322:     >> 2244 LOAD_GLOBAL          (agreement)
#             2246 LOAD_METHOD          (ARGB_ten_effect)
#             2248 LOAD_GLOBAL          (port)
#             2250 LOAD_ATTR            (ARGB_ser)
#             2252 LOAD_CONST           (254)
#             2254 LOAD_CONST           (254)
#             2256 CALL_METHOD          (3 positional arguments)
#             2258 POP_TOP
# 
# 23323:        2260 LOAD_FAST            (effect)
#             2262 BUILD_LIST           1
#             2264 LOAD_GLOBAL          (port)
#             2266 LOAD_ATTR            (ARGB_qun_async_config)
#             2268 LOAD_FAST            (effect)
#             2270 BINARY_SUBSCR
#             2272 BINARY_ADD
#             2274 STORE_FAST           (effect_id)
# 
# 23324:        2276 LOAD_FAST            (self)
#             2278 LOAD_METHOD          (fan_effect_set_sync)
#             2280 LOAD_FAST            (effect_id)
#             2282 CALL_METHOD          (1 positional argument)
#             2284 POP_TOP
#             2286 EXTENDED_ARG         (1024)
#             2288 JUMP_FORWARD         (to 3542)
# 
# 23325:     >> 2290 LOAD_FAST            (async_index)
#             2292 LOAD_CONST           (2)
#             2294 COMPARE_OP           (==)
#             2296 EXTENDED_ARG         (2304)
#             2298 POP_JUMP_IF_FALSE    (to 2456)
# 
# 23326:        2300 LOAD_CONST           (0)
#             2302 STORE_FAST           (effect)
# 
# 23327:        2304 LOAD_FAST            (tongp_shiy_index)
#             2306 LOAD_CONST           (0)
#             2308 COMPARE_OP           (==)
#             2310 EXTENDED_ARG         (2304)
#             2312 POP_JUMP_IF_FALSE    (to 2330)
# 
# 23328:        2314 LOAD_CONST           (20)
#             2316 STORE_FAST           (effect)
# 
# 23329:        2318 LOAD_FAST            (self)
#             2320 LOAD_METHOD          (tese_tongping_shiyin_xianshi)
#             2322 LOAD_CONST           (1)
#             2324 CALL_METHOD          (1 positional argument)
#             2326 POP_TOP
#             2328 JUMP_FORWARD         (to 2354)
# 
# 23330:     >> 2330 LOAD_FAST            (tongp_shiy_index)
#             2332 LOAD_CONST           (1)
#             2334 COMPARE_OP           (==)
#             2336 EXTENDED_ARG         (2304)
#             2338 POP_JUMP_IF_FALSE    (to 2354)
# 
# 23331:        2340 LOAD_CONST           (21)
#             2342 STORE_FAST           (effect)
# 
# 23332:        2344 LOAD_FAST            (self)
#             2346 LOAD_METHOD          (tese_tongping_shiyin_xianshi)
#             2348 LOAD_CONST           (2)
#             2350 CALL_METHOD          (1 positional argument)
#             2352 POP_TOP
# 
# 23333:     >> 2354 LOAD_FAST            (self)
#             2356 LOAD_METHOD          (fan_effect_settings)
#             2358 LOAD_FAST            (effect)
#             2360 CALL_METHOD          (1 positional argument)
#             2362 POP_TOP
# 
# 23334:        2364 LOAD_FAST            (effect)
#             2366 BUILD_LIST           1
#             2368 LOAD_GLOBAL          (port)
#             2370 LOAD_ATTR            (ARGB_qun_async_config)
#             2372 LOAD_FAST            (effect)
#             2374 BINARY_SUBSCR
#             2376 BINARY_ADD
#             2378 STORE_FAST           (effect_id)
# 
# 23335:        2380 LOAD_FAST            (self)
#             2382 LOAD_METHOD          (fan_effect_set_sync)
#             2384 LOAD_FAST            (effect_id)
#             2386 CALL_METHOD          (1 positional argument)
#             2388 POP_TOP
# 
# 23336:        2390 LOAD_FAST            (tongp_shiy_index)
#             2392 LOAD_GLOBAL          (test)
#             2394 STORE_ATTR           (fan_screen_shiyin_effect)
# 
# 23337:        2396 LOAD_GLOBAL          (test)
#             2398 LOAD_ATTR            (fan_shiyin_event)
#             2400 LOAD_METHOD          (set)
#             2402 CALL_METHOD          (0 positional arguments)
#             2404 POP_TOP
# 
# 23338:        2406 LOAD_GLOBAL          (time)
#             2408 LOAD_METHOD          (sleep)
#             2410 LOAD_CONST           (0.01)
#             2412 CALL_METHOD          (1 positional argument)
#             2414 POP_TOP
# 
# 23339:        2416 LOAD_GLOBAL          (test)
#             2418 LOAD_ATTR            (fan_shiyin_event)
#             2420 LOAD_METHOD          (clear)
#             2422 CALL_METHOD          (0 positional arguments)
#             2424 POP_TOP
# 
# 23340:        2426 LOAD_GLOBAL          (threading)
#             2428 LOAD_ATTR            (Thread)
#             2430 LOAD_CONST           (<Code38 code object <lambda> at 0x2589185a890, file Sync_light.py>, line 23340)
#             2432 LOAD_CONST           ("MainWindow.fan_dengxiao_index.<locals>.<lambda>")
#             2434 MAKE_FUNCTION        (No arguments)
#             2436 LOAD_CONST           (True)
#             2438 LOAD_CONST           (('target', 'daemon'))
#             2440 CALL_FUNCTION_KW     (2 total positional and keyword args)
#             2442 STORE_FAST           (t)
# 
# 23341:        2444 LOAD_FAST            (t)
#             2446 LOAD_METHOD          (start)
#             2448 CALL_METHOD          (0 positional arguments)
#             2450 POP_TOP
#             2452 EXTENDED_ARG         (1024)
#             2454 JUMP_FORWARD         (to 3542)
# 
# 23343:     >> 2456 LOAD_FAST            (fan_index)
#             2458 LOAD_CONST           (255)
#             2460 COMPARE_OP           (!=)
#             2462 EXTENDED_ARG         (3072)
#             2464 POP_JUMP_IF_FALSE    (to 3312)
#             2466 LOAD_FAST            (fan_index)
#             2468 LOAD_CONST           (10)
#             2470 COMPARE_OP           (<)
#             2472 EXTENDED_ARG         (3072)
#             2474 POP_JUMP_IF_FALSE    (to 3312)
# 
# 23344:        2476 LOAD_FAST            (self)
#             2478 LOAD_ATTR            (ARGB_FAN_all)
#             2480 LOAD_METHOD          (setStyleSheet)
# 
# 23345:        2482 LOAD_CONST           ("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
# 
# 23344:        2484 CALL_METHOD          (1 positional argument)
#             2486 POP_TOP
# 
# 23353:        2488 LOAD_FAST            (self)
#             2490 LOAD_ATTR            (ARGB_FAN_sync_all)
#             2492 LOAD_METHOD          (setStyleSheet)
# 
# 23354:        2494 LOAD_CONST           ("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
# 
# 23353:        2496 CALL_METHOD          (1 positional argument)
#             2498 POP_TOP
# 
# 23362:        2500 LOAD_GLOBAL          (range)
#             2502 LOAD_CONST           (10)
#             2504 CALL_FUNCTION        (1 positional argument)
#             2506 GET_ITER
#          >> 2508 FOR_ITER             (to 2638)
#             2510 STORE_FAST           (i)
# 
# 23363:        2512 LOAD_GLOBAL          (port)
#             2514 LOAD_ATTR            (ARGB_TEN_NUM)
#             2516 LOAD_FAST            (i)
#             2518 BINARY_SUBSCR
#             2520 LOAD_CONST           (0)
#             2522 COMPARE_OP           (!=)
#             2524 EXTENDED_ARG         (2560)
#             2526 POP_JUMP_IF_FALSE    (to 2616)
# 
# 23364:        2528 LOAD_FAST            (i)
#             2530 LOAD_FAST            (fan_index)
#             2532 COMPARE_OP           (==)
#             2534 EXTENDED_ARG         (2560)
#             2536 POP_JUMP_IF_FALSE    (to 2578)
# 
# 23365:        2538 LOAD_FAST            (self)
#             2540 LOAD_ATTR            (ARGB_FAN_devices)
#             2542 LOAD_FAST            (i)
#             2544 BINARY_SUBSCR
#             2546 LOAD_METHOD          (setIcon)
# 
# 23366:        2548 LOAD_GLOBAL          (QIcon)
#             2550 LOAD_GLOBAL          (port)
#             2552 LOAD_METHOD          (file_path_img)
#             2554 LOAD_GLOBAL          (port)
#             2556 LOAD_METHOD          (file_path_img)
#             2558 LOAD_FAST            (self)
#             2560 LOAD_ATTR            (ARGB_FAN_devices_img_xuanzhong)
#             2562 LOAD_FAST            (i)
#             2564 BINARY_SUBSCR
#             2566 CALL_METHOD          (1 positional argument)
#             2568 CALL_METHOD          (1 positional argument)
#             2570 CALL_FUNCTION        (1 positional argument)
# 
# 23365:        2572 CALL_METHOD          (1 positional argument)
#             2574 POP_TOP
#             2576 JUMP_FORWARD         (to 2616)
# 
# 23368:     >> 2578 LOAD_FAST            (self)
#             2580 LOAD_ATTR            (ARGB_FAN_devices)
#             2582 LOAD_FAST            (i)
#             2584 BINARY_SUBSCR
#             2586 LOAD_METHOD          (setIcon)
# 
# 23369:        2588 LOAD_GLOBAL          (QIcon)
#             2590 LOAD_GLOBAL          (port)
#             2592 LOAD_METHOD          (file_path_img)
#             2594 LOAD_GLOBAL          (port)
#             2596 LOAD_METHOD          (file_path_img)
#             2598 LOAD_FAST            (self)
#             2600 LOAD_ATTR            (ARGB_FAN_devices_img)
#             2602 LOAD_FAST            (i)
#             2604 BINARY_SUBSCR
#             2606 CALL_METHOD          (1 positional argument)
#             2608 CALL_METHOD          (1 positional argument)
#             2610 CALL_FUNCTION        (1 positional argument)
# 
# 23368:        2612 CALL_METHOD          (1 positional argument)
#             2614 POP_TOP
# 
# 23371:     >> 2616 LOAD_GLOBAL          (copy)
#             2618 LOAD_METHOD          (deepcopy)
#             2620 LOAD_GLOBAL          (port)
#             2622 LOAD_ATTR            (ARGB_TEN_MODEL1)
#             2624 LOAD_FAST            (i)
#             2626 BINARY_SUBSCR
#             2628 CALL_METHOD          (1 positional argument)
#             2630 LOAD_GLOBAL          (agreement)
#             2632 STORE_ATTR           (fan_order_list)
#             2634 EXTENDED_ARG         (2304)
#             2636 JUMP_ABSOLUTE        (to 2508)
# 
# 23372:     >> 2638 LOAD_GLOBAL          (copy)
#             2640 LOAD_METHOD          (deepcopy)
#             2642 LOAD_FAST            (fan_index)
#             2644 CALL_METHOD          (1 positional argument)
#             2646 STORE_FAST           (index)
# 
# 23373:        2648 LOAD_GLOBAL          (port)
#             2650 LOAD_ATTR            (gaoji_ten_dengxiao_leixing)
#             2652 LOAD_FAST            (index)
#             2654 BINARY_SUBSCR
#             2656 LOAD_CONST           (2)
#             2658 BINARY_SUBSCR
#             2660 STORE_FAST           (fan_id)
# 
# 23374:        2662 LOAD_GLOBAL          (port)
#             2664 LOAD_ATTR            (ARGB_TEN_LEIXING)
#             2666 LOAD_FAST            (index)
#             2668 BINARY_SUBSCR
#             2670 LOAD_CONST           (1)
#             2672 DUP_TOP
#             2674 ROT_THREE
#             2676 COMPARE_OP           (==)
#             2678 EXTENDED_ARG         (2560)
#             2680 POP_JUMP_IF_FALSE    (to 2694)
#             2682 LOAD_GLOBAL          (port)
#             2684 LOAD_ATTR            (jimu_lianpai)
#             2686 COMPARE_OP           (in)
#             2688 EXTENDED_ARG         (2816)
#             2690 POP_JUMP_IF_FALSE    (to 2904)
#             2692 JUMP_FORWARD         (to 2698)
#          >> 2694 POP_TOP
#             2696 JUMP_FORWARD         (to 2904)
# 
# 23375:     >> 2698 LOAD_GLOBAL          (port)
#             2700 LOAD_ATTR            (ARGB_TEN_MODEL1)
#             2702 LOAD_FAST            (index)
#             2704 BINARY_SUBSCR
#             2706 LOAD_FAST            (fan_id)
#             2708 BINARY_SUBSCR
#             2710 LOAD_CONST           (0)
#             2712 BINARY_SUBSCR
#             2714 STORE_FAST           (effect)
# 
# 23376:        2716 LOAD_GLOBAL          (print)
#             2718 LOAD_CONST           ("fan_id : ")
#             2720 LOAD_FAST            (fan_id)
#             2722 FORMAT_VALUE         0
#             2724 BUILD_STRING         2
#             2726 CALL_FUNCTION        (1 positional argument)
#             2728 POP_TOP
# 
# 23377:        2730 LOAD_FAST            (self)
#             2732 LOAD_ATTR            (ARGB_FAN_chuan)
#             2734 GET_ITER
#          >> 2736 FOR_ITER             (to 2752)
#             2738 STORE_FAST           (i)
# 
# 23378:        2740 LOAD_FAST            (i)
#             2742 LOAD_METHOD          (show)
#             2744 CALL_METHOD          (0 positional arguments)
#             2746 POP_TOP
#             2748 EXTENDED_ARG         (2560)
#             2750 JUMP_ABSOLUTE        (to 2736)
# 
# 23379:     >> 2752 LOAD_GLOBAL          (range)
#             2754 LOAD_CONST           (6)
#             2756 CALL_FUNCTION        (1 positional argument)
#             2758 GET_ITER
#          >> 2760 FOR_ITER             (to 2872)
#             2762 STORE_FAST           (i)
# 
# 23380:        2764 LOAD_FAST            (i)
#             2766 LOAD_FAST            (fan_id)
#             2768 COMPARE_OP           (!=)
#             2770 EXTENDED_ARG         (2816)
#             2772 POP_JUMP_IF_FALSE    (to 2830)
# 
# 23381:        2774 LOAD_FAST            (self)
#             2776 LOAD_ATTR            (ARGB_FAN_chuan)
#             2778 LOAD_FAST            (i)
#             2780 BINARY_SUBSCR
#             2782 LOAD_METHOD          (setIcon)
#             2784 LOAD_GLOBAL          (QIcon)
#             2786 LOAD_GLOBAL          (port)
#             2788 LOAD_METHOD          (file_path_img)
#             2790 LOAD_GLOBAL          (port)
#             2792 LOAD_METHOD          (file_path_img)
#             2794 LOAD_FAST            (self)
#             2796 LOAD_ATTR            (ARGB_FAN_chuan_img)
#             2798 LOAD_FAST            (i)
#             2800 BINARY_SUBSCR
#             2802 CALL_METHOD          (1 positional argument)
#             2804 CALL_METHOD          (1 positional argument)
#             2806 CALL_FUNCTION        (1 positional argument)
#             2808 CALL_METHOD          (1 positional argument)
#             2810 POP_TOP
# 
# 23382:        2812 LOAD_FAST            (self)
#             2814 LOAD_ATTR            (ARGB_FAN_chuan)
#             2816 LOAD_FAST            (i)
#             2818 BINARY_SUBSCR
#             2820 LOAD_METHOD          (setChecked)
#             2822 LOAD_CONST           (False)
#             2824 CALL_METHOD          (1 positional argument)
#             2826 POP_TOP
#             2828 JUMP_FORWARD         (to 2868)
# 
# 23384:     >> 2830 LOAD_FAST            (self)
#             2832 LOAD_ATTR            (ARGB_FAN_chuan)
#             2834 LOAD_FAST            (i)
#             2836 BINARY_SUBSCR
#             2838 LOAD_METHOD          (setIcon)
# 
# 23385:        2840 LOAD_GLOBAL          (QIcon)
#             2842 LOAD_GLOBAL          (port)
#             2844 LOAD_METHOD          (file_path_img)
#             2846 LOAD_GLOBAL          (port)
#             2848 LOAD_METHOD          (file_path_img)
#             2850 LOAD_FAST            (self)
#             2852 LOAD_ATTR            (ARGB_FAN_chuan_img_xuanzhong)
#             2854 LOAD_FAST            (i)
#             2856 BINARY_SUBSCR
#             2858 CALL_METHOD          (1 positional argument)
#             2860 CALL_METHOD          (1 positional argument)
#             2862 CALL_FUNCTION        (1 positional argument)
# 
# 23384:        2864 CALL_METHOD          (1 positional argument)
#             2866 POP_TOP
#          >> 2868 EXTENDED_ARG         (2560)
#             2870 JUMP_ABSOLUTE        (to 2760)
# 
# 23386:     >> 2872 LOAD_FAST            (self)
#             2874 LOAD_METHOD          (fan_effect_settings)
#             2876 LOAD_FAST            (effect)
#             2878 CALL_METHOD          (1 positional argument)
#             2880 POP_TOP
# 
# 23387:        2882 LOAD_FAST            (self)
#             2884 LOAD_METHOD          (fan_effect_set_sync)
#             2886 LOAD_GLOBAL          (port)
#             2888 LOAD_ATTR            (ARGB_TEN_MODEL1)
#             2890 LOAD_FAST            (index)
#             2892 BINARY_SUBSCR
#             2894 LOAD_FAST            (fan_id)
#             2896 BINARY_SUBSCR
#             2898 CALL_METHOD          (1 positional argument)
#             2900 POP_TOP
#             2902 JUMP_FORWARD         (to 2986)
# 
# 23389:     >> 2904 LOAD_CONST           (5)
#             2906 STORE_FAST           (fan_id)
# 
# 23390:        2908 LOAD_GLOBAL          (enumerate)
#             2910 LOAD_FAST            (self)
#             2912 LOAD_ATTR            (ARGB_FAN_chuan)
#             2914 CALL_FUNCTION        (1 positional argument)
#             2916 GET_ITER
#          >> 2918 FOR_ITER             (to 2938)
#             2920 UNPACK_SEQUENCE      2
#             2922 STORE_FAST           (i)
#             2924 STORE_FAST           (vaule)
# 
# 23391:        2926 LOAD_FAST            (vaule)
#             2928 LOAD_METHOD          (hide)
#             2930 CALL_METHOD          (0 positional arguments)
#             2932 POP_TOP
#             2934 EXTENDED_ARG         (2816)
#             2936 JUMP_ABSOLUTE        (to 2918)
# 
# 23392:     >> 2938 LOAD_GLOBAL          (port)
#             2940 LOAD_ATTR            (ARGB_TEN_MODEL1)
#             2942 LOAD_FAST            (index)
#             2944 BINARY_SUBSCR
#             2946 LOAD_FAST            (fan_id)
#             2948 BINARY_SUBSCR
#             2950 LOAD_CONST           (0)
#             2952 BINARY_SUBSCR
#             2954 STORE_FAST           (effect)
# 
# 23393:        2956 LOAD_FAST            (self)
#             2958 LOAD_METHOD          (fan_effect_settings)
#             2960 LOAD_FAST            (effect)
#             2962 CALL_METHOD          (1 positional argument)
#             2964 POP_TOP
# 
# 23394:        2966 LOAD_FAST            (self)
#             2968 LOAD_METHOD          (fan_effect_set_sync)
#             2970 LOAD_GLOBAL          (port)
#             2972 LOAD_ATTR            (ARGB_TEN_MODEL1)
#             2974 LOAD_FAST            (index)
#             2976 BINARY_SUBSCR
#             2978 LOAD_FAST            (fan_id)
#             2980 BINARY_SUBSCR
#             2982 CALL_METHOD          (1 positional argument)
#             2984 POP_TOP
# 
# 23395:     >> 2986 LOAD_FAST            (fan_id)
#             2988 LOAD_CONST           (5)
#             2990 COMPARE_OP           (!=)
#             2992 EXTENDED_ARG         (2816)
#             2994 POP_JUMP_IF_FALSE    (to 3018)
# 
# 23396:        2996 LOAD_FAST            (self)
#             2998 LOAD_ATTR            (ARGB_FAN_all)
#             3000 LOAD_METHOD          (hide)
#             3002 CALL_METHOD          (0 positional arguments)
#             3004 POP_TOP
# 
# 23397:        3006 LOAD_FAST            (self)
#             3008 LOAD_ATTR            (ARGB_FAN_sync_all)
#             3010 LOAD_METHOD          (hide)
#             3012 CALL_METHOD          (0 positional arguments)
#             3014 POP_TOP
#             3016 JUMP_FORWARD         (to 3076)
# 
# 23399:     >> 3018 LOAD_FAST            (self)
#             3020 LOAD_ATTR            (anyc_list)
#             3022 LOAD_FAST            (effect)
#             3024 BINARY_SUBSCR
#             3026 LOAD_CONST           (255)
#             3028 COMPARE_OP           (!=)
#             3030 EXTENDED_ARG         (2816)
#             3032 POP_JUMP_IF_FALSE    (to 3056)
# 
# 23400:        3034 LOAD_FAST            (self)
#             3036 LOAD_ATTR            (ARGB_FAN_all)
#             3038 LOAD_METHOD          (show)
#             3040 CALL_METHOD          (0 positional arguments)
#             3042 POP_TOP
# 
# 23401:        3044 LOAD_FAST            (self)
#             3046 LOAD_ATTR            (ARGB_FAN_sync_all)
#             3048 LOAD_METHOD          (show)
#             3050 CALL_METHOD          (0 positional arguments)
#             3052 POP_TOP
#             3054 JUMP_FORWARD         (to 3076)
# 
# 23403:     >> 3056 LOAD_FAST            (self)
#             3058 LOAD_ATTR            (ARGB_FAN_all)
#             3060 LOAD_METHOD          (show)
#             3062 CALL_METHOD          (0 positional arguments)
#             3064 POP_TOP
# 
# 23404:        3066 LOAD_FAST            (self)
#             3068 LOAD_ATTR            (ARGB_FAN_sync_all)
#             3070 LOAD_METHOD          (hide)
#             3072 CALL_METHOD          (0 positional arguments)
#             3074 POP_TOP
# 
# 23409:     >> 3076 LOAD_FAST            (self)
#             3078 LOAD_METHOD          (dan_fan_huifu_tab_effect)
#             3080 LOAD_FAST            (fan_index)
#             3082 CALL_METHOD          (1 positional argument)
#             3084 POP_TOP
# 
# 23410:        3086 LOAD_GLOBAL          (port)
#             3088 LOAD_ATTR            (gaoji_ten_dengxiao_leixing)
#             3090 LOAD_FAST            (fan_index)
#             3092 BINARY_SUBSCR
#             3094 LOAD_CONST           (2)
#             3096 BINARY_SUBSCR
#             3098 STORE_FAST           (fan_id)
# 
# 23411:        3100 LOAD_GLOBAL          (port)
#             3102 LOAD_ATTR            (ARGB_TEN_LEIXING)
#             3104 LOAD_FAST            (fan_index)
#             3106 BINARY_SUBSCR
#             3108 LOAD_CONST           (1)
#             3110 DUP_TOP
#             3112 ROT_THREE
#             3114 COMPARE_OP           (==)
#             3116 EXTENDED_ARG         (3072)
#             3118 POP_JUMP_IF_FALSE    (to 3132)
#             3120 LOAD_GLOBAL          (port)
#             3122 LOAD_ATTR            (jimu_lianpai)
#             3124 COMPARE_OP           (in)
#             3126 EXTENDED_ARG         (3328)
#             3128 POP_JUMP_IF_FALSE    (to 3542)
#             3130 JUMP_FORWARD         (to 3136)
#          >> 3132 POP_TOP
#             3134 JUMP_FORWARD         (to 3310)
# 
# 23412:     >> 3136 LOAD_GLOBAL          (port)
#             3138 LOAD_ATTR            (ARGB_TEN_MODEL1)
#             3140 LOAD_FAST            (fan_index)
#             3142 BINARY_SUBSCR
#             3144 LOAD_FAST            (fan_id)
#             3146 BINARY_SUBSCR
#             3148 LOAD_CONST           (0)
#             3150 BINARY_SUBSCR
#             3152 STORE_FAST           (effect)
# 
# 23413:        3154 LOAD_GLOBAL          (print)
#             3156 LOAD_CONST           ("fan_id : ")
#             3158 LOAD_FAST            (fan_id)
#             3160 FORMAT_VALUE         0
#             3162 BUILD_STRING         2
#             3164 CALL_FUNCTION        (1 positional argument)
#             3166 POP_TOP
# 
# 23415:        3168 LOAD_FAST            (self)
#             3170 LOAD_ATTR            (ARGB_FAN_chuan)
#             3172 GET_ITER
#          >> 3174 FOR_ITER             (to 3190)
#             3176 STORE_FAST           (i)
# 
# 23416:        3178 LOAD_FAST            (i)
#             3180 LOAD_METHOD          (show)
#             3182 CALL_METHOD          (0 positional arguments)
#             3184 POP_TOP
#             3186 EXTENDED_ARG         (3072)
#             3188 JUMP_ABSOLUTE        (to 3174)
# 
# 23417:     >> 3190 LOAD_GLOBAL          (range)
#             3192 LOAD_CONST           (6)
#             3194 CALL_FUNCTION        (1 positional argument)
#             3196 GET_ITER
#          >> 3198 FOR_ITER             (to 3310)
#             3200 STORE_FAST           (i)
# 
# 23418:        3202 LOAD_FAST            (i)
#             3204 LOAD_FAST            (fan_id)
#             3206 COMPARE_OP           (!=)
#             3208 EXTENDED_ARG         (3072)
#             3210 POP_JUMP_IF_FALSE    (to 3268)
# 
# 23419:        3212 LOAD_FAST            (self)
#             3214 LOAD_ATTR            (ARGB_FAN_chuan)
#             3216 LOAD_FAST            (i)
#             3218 BINARY_SUBSCR
#             3220 LOAD_METHOD          (setIcon)
#             3222 LOAD_GLOBAL          (QIcon)
#             3224 LOAD_GLOBAL          (port)
#             3226 LOAD_METHOD          (file_path_img)
#             3228 LOAD_GLOBAL          (port)
#             3230 LOAD_METHOD          (file_path_img)
#             3232 LOAD_FAST            (self)
#             3234 LOAD_ATTR            (ARGB_FAN_chuan_img)
#             3236 LOAD_FAST            (i)
#             3238 BINARY_SUBSCR
#             3240 CALL_METHOD          (1 positional argument)
#             3242 CALL_METHOD          (1 positional argument)
#             3244 CALL_FUNCTION        (1 positional argument)
#             3246 CALL_METHOD          (1 positional argument)
#             3248 POP_TOP
# 
# 23420:        3250 LOAD_FAST            (self)
#             3252 LOAD_ATTR            (ARGB_FAN_chuan)
#             3254 LOAD_FAST            (i)
#             3256 BINARY_SUBSCR
#             3258 LOAD_METHOD          (setChecked)
#             3260 LOAD_CONST           (False)
#             3262 CALL_METHOD          (1 positional argument)
#             3264 POP_TOP
#             3266 JUMP_FORWARD         (to 3306)
# 
# 23422:     >> 3268 LOAD_FAST            (self)
#             3270 LOAD_ATTR            (ARGB_FAN_chuan)
#             3272 LOAD_FAST            (i)
#             3274 BINARY_SUBSCR
#             3276 LOAD_METHOD          (setIcon)
# 
# 23423:        3278 LOAD_GLOBAL          (QIcon)
#             3280 LOAD_GLOBAL          (port)
#             3282 LOAD_METHOD          (file_path_img)
#             3284 LOAD_GLOBAL          (port)
#             3286 LOAD_METHOD          (file_path_img)
#             3288 LOAD_FAST            (self)
#             3290 LOAD_ATTR            (ARGB_FAN_chuan_img_xuanzhong)
#             3292 LOAD_FAST            (i)
#             3294 BINARY_SUBSCR
#             3296 CALL_METHOD          (1 positional argument)
#             3298 CALL_METHOD          (1 positional argument)
#             3300 CALL_FUNCTION        (1 positional argument)
# 
# 23422:        3302 CALL_METHOD          (1 positional argument)
#             3304 POP_TOP
#          >> 3306 EXTENDED_ARG         (3072)
#             3308 JUMP_ABSOLUTE        (to 3198)
#          >> 3310 JUMP_FORWARD         (to 3542)
# 
# 23424:     >> 3312 LOAD_FAST            (fan_index)
#             3314 LOAD_CONST           (255)
#             3316 COMPARE_OP           (==)
#             3318 EXTENDED_ARG         (3328)
#             3320 POP_JUMP_IF_FALSE    (to 3542)
# 
# 23425:        3322 LOAD_FAST            (self)
#             3324 LOAD_ATTR            (ARGB_FAN_all)
#             3326 LOAD_METHOD          (setStyleSheet)
# 
# 23426:        3328 LOAD_CONST           ("background-color: rgb(25,78,255);color: rgb(243,244,243);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
# 
# 23425:        3330 CALL_METHOD          (1 positional argument)
#             3332 POP_TOP
# 
# 23434:        3334 LOAD_FAST            (self)
#             3336 LOAD_ATTR            (ARGB_FAN_sync_all)
#             3338 LOAD_METHOD          (setStyleSheet)
# 
# 23435:        3340 LOAD_CONST           ("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
# 
# 23434:        3342 CALL_METHOD          (1 positional argument)
#             3344 POP_TOP
# 
# 23443:        3346 LOAD_GLOBAL          (range)
#             3348 LOAD_CONST           (10)
#             3350 CALL_FUNCTION        (1 positional argument)
#             3352 GET_ITER
#          >> 3354 FOR_ITER             (to 3400)
#             3356 STORE_FAST           (i)
# 
# 23444:        3358 LOAD_FAST            (self)
#             3360 LOAD_ATTR            (ARGB_FAN_devices)
#             3362 LOAD_FAST            (i)
#             3364 BINARY_SUBSCR
#             3366 LOAD_METHOD          (setIcon)
# 
# 23445:        3368 LOAD_GLOBAL          (QIcon)
#             3370 LOAD_GLOBAL          (port)
#             3372 LOAD_METHOD          (file_path_img)
#             3374 LOAD_GLOBAL          (port)
#             3376 LOAD_METHOD          (file_path_img)
#             3378 LOAD_FAST            (self)
#             3380 LOAD_ATTR            (ARGB_FAN_devices_img_xuanzhong)
#             3382 LOAD_FAST            (i)
#             3384 BINARY_SUBSCR
#             3386 CALL_METHOD          (1 positional argument)
#             3388 CALL_METHOD          (1 positional argument)
#             3390 CALL_FUNCTION        (1 positional argument)
# 
# 23444:        3392 CALL_METHOD          (1 positional argument)
#             3394 POP_TOP
#             3396 EXTENDED_ARG         (3328)
#             3398 JUMP_ABSOLUTE        (to 3354)
# 
# 23446:     >> 3400 LOAD_FAST            (effect)
#             3402 LOAD_GLOBAL          (agreement)
#             3404 LOAD_ATTR            (fan_order_list)
#             3406 LOAD_CONST           (0)
#             3408 STORE_SUBSCR
# 
# 23447:        3410 LOAD_GLOBAL          (port)
#             3412 LOAD_ATTR            (ARGB_dan_sync_config_gaoji)
#             3414 LOAD_FAST            (effect)
#             3416 BINARY_SUBSCR
#             3418 LOAD_CONST           (0)
#             3420 BINARY_SUBSCR
#             3422 LOAD_GLOBAL          (agreement)
#             3424 LOAD_ATTR            (fan_order_list)
#             3426 LOAD_CONST           (1)
#             3428 STORE_SUBSCR
# 
# 23448:        3430 LOAD_GLOBAL          (port)
#             3432 LOAD_ATTR            (ARGB_dan_sync_config_gaoji)
#             3434 LOAD_FAST            (effect)
#             3436 BINARY_SUBSCR
#             3438 LOAD_CONST           (1)
#             3440 BINARY_SUBSCR
#             3442 LOAD_GLOBAL          (agreement)
#             3444 LOAD_ATTR            (fan_order_list)
#             3446 LOAD_CONST           (2)
#             3448 STORE_SUBSCR
# 
# 23449:        3450 LOAD_GLOBAL          (port)
#             3452 LOAD_ATTR            (ARGB_dan_sync_config_gaoji)
#             3454 LOAD_FAST            (effect)
#             3456 BINARY_SUBSCR
#             3458 LOAD_CONST           (2)
#             3460 BINARY_SUBSCR
#             3462 LOAD_GLOBAL          (agreement)
#             3464 LOAD_ATTR            (fan_order_list)
#             3466 LOAD_CONST           (3)
#             3468 STORE_SUBSCR
# 
# 23450:        3470 LOAD_GLOBAL          (range)
#             3472 LOAD_CONST           (12)
#             3474 CALL_FUNCTION        (1 positional argument)
#             3476 GET_ITER
#          >> 3478 FOR_ITER             (to 3514)
#             3480 STORE_FAST           (i)
# 
# 23451:        3482 LOAD_GLOBAL          (port)
#             3484 LOAD_ATTR            (ARGB_dan_sync_config_gaoji)
#             3486 LOAD_FAST            (effect)
#             3488 BINARY_SUBSCR
#             3490 LOAD_CONST           (3)
#             3492 LOAD_FAST            (i)
#             3494 BINARY_ADD
#             3496 BINARY_SUBSCR
#             3498 LOAD_GLOBAL          (agreement)
#             3500 LOAD_ATTR            (fan_order_list)
#             3502 LOAD_CONST           (4)
#             3504 LOAD_FAST            (i)
#             3506 BINARY_ADD
#             3508 STORE_SUBSCR
#             3510 EXTENDED_ARG         (3328)
#             3512 JUMP_ABSOLUTE        (to 3478)
# 
# 23452:     >> 3514 LOAD_GLOBAL          (agreement)
#             3516 LOAD_METHOD          (ARGB_ten_effect)
#             3518 LOAD_GLOBAL          (port)
#             3520 LOAD_ATTR            (ARGB_ser)
#             3522 LOAD_CONST           (255)
#             3524 LOAD_CONST           (255)
#             3526 CALL_METHOD          (3 positional arguments)
#             3528 POP_TOP
# 
# 23453:        3530 LOAD_FAST            (self)
#             3532 LOAD_METHOD          (fan_effect_set_sync)
#             3534 LOAD_GLOBAL          (agreement)
#             3536 LOAD_ATTR            (fan_order_list)
#             3538 CALL_METHOD          (1 positional argument)
#             3540 POP_TOP
# 
# 23455:     >> 3542 LOAD_GLOBAL          (range)
#             3544 LOAD_CONST           (10)
#             3546 CALL_FUNCTION        (1 positional argument)
#             3548 GET_ITER
#          >> 3550 FOR_ITER             (to 3634)
#             3552 STORE_FAST           (i)
# 
# 23456:        3554 LOAD_GLOBAL          (port)
#             3556 LOAD_ATTR            (ARGB_TEN_NUM)
#             3558 LOAD_FAST            (i)
#             3560 BINARY_SUBSCR
#             3562 LOAD_CONST           (0)
#             3564 COMPARE_OP           (!=)
#             3566 EXTENDED_ARG         (3584)
#             3568 POP_JUMP_IF_FALSE    (to 3586)
# 
# 23457:        3570 LOAD_FAST            (self)
#             3572 LOAD_ATTR            (ARGB_FAN_SN_NUM)
#             3574 LOAD_FAST            (i)
#             3576 BINARY_SUBSCR
#             3578 LOAD_METHOD          (show)
#             3580 CALL_METHOD          (0 positional arguments)
#             3582 POP_TOP
#             3584 JUMP_FORWARD         (to 3600)
# 
# 23459:     >> 3586 LOAD_FAST            (self)
#             3588 LOAD_ATTR            (ARGB_FAN_SN_NUM)
#             3590 LOAD_FAST            (i)
#             3592 BINARY_SUBSCR
#             3594 LOAD_METHOD          (hide)
#             3596 CALL_METHOD          (0 positional arguments)
#             3598 POP_TOP
# 
# 23460:     >> 3600 LOAD_GLOBAL          (port)
#             3602 LOAD_ATTR            (ARGB_TEN_NUM)
#             3604 LOAD_CONST           (0)
#             3606 BINARY_SUBSCR
#             3608 LOAD_CONST           (6)
#             3610 COMPARE_OP           (==)
#             3612 EXTENDED_ARG         (3328)
#             3614 POP_JUMP_IF_FALSE    (to 3550)
# 
# 23461:        3616 LOAD_FAST            (self)
#             3618 LOAD_ATTR            (ARGB_FAN_SN_NUM)
#             3620 LOAD_CONST           (0)
#             3622 BINARY_SUBSCR
#             3624 LOAD_METHOD          (hide)
#             3626 CALL_METHOD          (0 positional arguments)
#             3628 POP_TOP
#             3630 EXTENDED_ARG         (3328)
#             3632 JUMP_ABSOLUTE        (to 3550)
# 
# 23464:     >> 3634 LOAD_GLOBAL          (port)
#             3636 LOAD_ATTR            (fan_argb_button_model_index)
#             3638 LOAD_CONST           (4)
#             3640 COMPARE_OP           (==)
#             3642 EXTENDED_ARG         (4608)
#             3644 POP_JUMP_IF_FALSE    (to 4710)
# 
# 23465:        3646 LOAD_FAST            (self)
#             3648 LOAD_ATTR            (main_widget_mubu)
#             3650 LOAD_METHOD          (hide)
#             3652 CALL_METHOD          (0 positional arguments)
#             3654 POP_TOP
# 
# 23468:        3656 LOAD_GLOBAL          (print)
#             3658 LOAD_CONST           ("硬件设置选项卡")
#             3660 CALL_FUNCTION        (1 positional argument)
#             3662 POP_TOP
# 
# 23469:        3664 LOAD_FAST            (self)
#             3666 LOAD_ATTR            (jiyi_moshi)
#             3668 LOAD_METHOD          (show)
#             3670 CALL_METHOD          (0 positional arguments)
#             3672 POP_TOP
# 
# 23470:        3674 LOAD_FAST            (self)
#             3676 LOAD_ATTR            (jiyi_changer_switch)
#             3678 LOAD_METHOD          (show)
#             3680 CALL_METHOD          (0 positional arguments)
#             3682 POP_TOP
# 
# 23471:        3684 LOAD_FAST            (self)
#             3686 LOAD_ATTR            (guding_moshi)
#             3688 LOAD_METHOD          (show)
#             3690 CALL_METHOD          (0 positional arguments)
#             3692 POP_TOP
# 
# 23472:        3694 LOAD_FAST            (self)
#             3696 LOAD_ATTR            (guding_changer_switch)
#             3698 LOAD_METHOD          (show)
#             3700 CALL_METHOD          (0 positional arguments)
#             3702 POP_TOP
# 
# 23473:        3704 LOAD_FAST            (self)
#             3706 LOAD_ATTR            (kaijizijian)
#             3708 LOAD_METHOD          (show)
#             3710 CALL_METHOD          (0 positional arguments)
#             3712 POP_TOP
# 
# 23474:        3714 LOAD_FAST            (self)
#             3716 LOAD_ATTR            (kaijizijian_changer_switch)
#             3718 LOAD_METHOD          (show)
#             3720 CALL_METHOD          (0 positional arguments)
#             3722 POP_TOP
# 
# 23475:        3724 LOAD_GLOBAL          (port)
#             3726 LOAD_ATTR            (KaiJi_ZiJian)
#             3728 LOAD_CONST           (1)
#             3730 COMPARE_OP           (==)
#             3732 EXTENDED_ARG         (3584)
#             3734 POP_JUMP_IF_FALSE    (to 3750)
# 
# 23476:        3736 LOAD_FAST            (self)
#             3738 LOAD_ATTR            (kaijizijian_changer_switch)
#             3740 LOAD_METHOD          (setChecked)
#             3742 LOAD_CONST           (False)
#             3744 CALL_METHOD          (1 positional argument)
#             3746 POP_TOP
#             3748 JUMP_FORWARD         (to 3774)
# 
# 23477:     >> 3750 LOAD_GLOBAL          (port)
#             3752 LOAD_ATTR            (KaiJi_ZiJian)
#             3754 LOAD_CONST           (255)
#             3756 COMPARE_OP           (==)
#             3758 EXTENDED_ARG         (3584)
#             3760 POP_JUMP_IF_FALSE    (to 3774)
# 
# 23478:        3762 LOAD_FAST            (self)
#             3764 LOAD_ATTR            (kaijizijian_changer_switch)
#             3766 LOAD_METHOD          (setChecked)
#             3768 LOAD_CONST           (True)
#             3770 CALL_METHOD          (1 positional argument)
#             3772 POP_TOP
# 
# 23479:     >> 3774 LOAD_GLOBAL          (port)
#             3776 LOAD_ATTR            (On_Effect)
#             3778 LOAD_CONST           (0)
#             3780 BINARY_SUBSCR
#             3782 LOAD_CONST           (1)
#             3784 COMPARE_OP           (==)
#             3786 EXTENDED_ARG         (3840)
#             3788 POP_JUMP_IF_FALSE    (to 4014)
# 
# 23483:        3790 LOAD_FAST            (self)
#             3792 LOAD_ATTR            (liusu_fan_lable)
#             3794 LOAD_METHOD          (show)
#             3796 CALL_METHOD          (0 positional arguments)
#             3798 POP_TOP
# 
# 23484:        3800 LOAD_FAST            (self)
#             3802 LOAD_ATTR            (slider_fan_liusu)
#             3804 LOAD_METHOD          (show)
#             3806 CALL_METHOD          (0 positional arguments)
#             3808 POP_TOP
# 
# 23485:        3810 LOAD_FAST            (self)
#             3812 LOAD_ATTR            (liusu_fan_text_lable)
#             3814 LOAD_METHOD          (show)
#             3816 CALL_METHOD          (0 positional arguments)
#             3818 POP_TOP
# 
# 23487:        3820 LOAD_FAST            (self)
#             3822 LOAD_ATTR            (fangxiang_fan_lable)
#             3824 LOAD_METHOD          (show)
#             3826 CALL_METHOD          (0 positional arguments)
#             3828 POP_TOP
# 
# 23488:        3830 LOAD_FAST            (self)
#             3832 LOAD_ATTR            (fan_fangxiang_buttons)
#             3834 GET_ITER
#          >> 3836 FOR_ITER             (to 3852)
#             3838 STORE_FAST           (i)
# 
# 23489:        3840 LOAD_FAST            (i)
#             3842 LOAD_METHOD          (hide)
#             3844 CALL_METHOD          (0 positional arguments)
#             3846 POP_TOP
#             3848 EXTENDED_ARG         (3584)
#             3850 JUMP_ABSOLUTE        (to 3836)
# 
# 23491:     >> 3852 LOAD_FAST            (self)
#             3854 LOAD_ATTR            (liangdu_fan_lable)
#             3856 LOAD_METHOD          (show)
#             3858 CALL_METHOD          (0 positional arguments)
#             3860 POP_TOP
# 
# 23492:        3862 LOAD_FAST            (self)
#             3864 LOAD_ATTR            (slider_fan_liangdu)
#             3866 LOAD_METHOD          (show)
#             3868 CALL_METHOD          (0 positional arguments)
#             3870 POP_TOP
# 
# 23493:        3872 LOAD_FAST            (self)
#             3874 LOAD_ATTR            (liangdu_fan_text_lable)
#             3876 LOAD_METHOD          (show)
#             3878 CALL_METHOD          (0 positional arguments)
#             3880 POP_TOP
# 
# 23495:        3882 LOAD_FAST            (self)
#             3884 LOAD_ATTR            (yanse_fan_lable)
#             3886 LOAD_METHOD          (show)
#             3888 CALL_METHOD          (0 positional arguments)
#             3890 POP_TOP
# 
# 23496:        3892 LOAD_FAST            (self)
#             3894 LOAD_ATTR            (fan_dengxiao_buttons)
#             3896 GET_ITER
#          >> 3898 FOR_ITER             (to 3914)
#             3900 STORE_FAST           (i)
# 
# 23497:        3902 LOAD_FAST            (i)
#             3904 LOAD_METHOD          (show)
#             3906 CALL_METHOD          (0 positional arguments)
#             3908 POP_TOP
#             3910 EXTENDED_ARG         (3840)
#             3912 JUMP_ABSOLUTE        (to 3898)
# 
# 23498:     >> 3914 LOAD_FAST            (self)
#             3916 LOAD_ATTR            (button_fan_tiaose)
#             3918 LOAD_METHOD          (show)
#             3920 CALL_METHOD          (0 positional arguments)
#             3922 POP_TOP
# 
# 23501:        3924 LOAD_FAST            (self)
#             3926 LOAD_ATTR            (jiyi_changer_switch)
#             3928 LOAD_METHOD          (setChecked)
#             3930 LOAD_CONST           (False)
#             3932 CALL_METHOD          (1 positional argument)
#             3934 POP_TOP
# 
# 23502:        3936 LOAD_FAST            (self)
#             3938 LOAD_ATTR            (guding_changer_switch)
#             3940 LOAD_METHOD          (setChecked)
#             3942 LOAD_CONST           (True)
#             3944 CALL_METHOD          (1 positional argument)
#             3946 POP_TOP
# 
# 23503:        3948 LOAD_CONST           (<Code38 code object <listcomp> at 0x2589185a7b0, file Sync_light.py>, line 23503)
#             3950 LOAD_CONST           ("MainWindow.fan_dengxiao_index.<locals>.<listcomp>")
#             3952 MAKE_FUNCTION        (No arguments)
#             3954 LOAD_GLOBAL          (range)
#             3956 LOAD_CONST           (52)
#             3958 CALL_FUNCTION        (1 positional argument)
#             3960 GET_ITER
#             3962 CALL_FUNCTION        (1 positional argument)
#             3964 LOAD_GLOBAL          (agreement)
#             3966 STORE_ATTR           (fan_order_list)
# 
# 23504:        3968 LOAD_GLOBAL          (port)
#             3970 LOAD_ATTR            (On_Effect)
#             3972 LOAD_CONST           (1)
#             3974 LOAD_CONST           (17)
#             3976 BUILD_SLICE          2
#             3978 BINARY_SUBSCR
#             3980 LOAD_GLOBAL          (agreement)
#             3982 LOAD_ATTR            (fan_order_list)
#             3984 LOAD_CONST           (0)
#             3986 LOAD_CONST           (16)
#             3988 BUILD_SLICE          2
#             3990 STORE_SUBSCR
# 
# 23505:        3992 LOAD_FAST            (self)
#             3994 LOAD_METHOD          (fan_effect_settings)
#             3996 LOAD_CONST           (22)
#             3998 CALL_METHOD          (1 positional argument)
#             4000 POP_TOP
# 
# 23506:        4002 LOAD_FAST            (self)
#             4004 LOAD_METHOD          (fan_effect_set_sync)
#             4006 LOAD_GLOBAL          (agreement)
#             4008 LOAD_ATTR            (fan_order_list)
#             4010 CALL_METHOD          (1 positional argument)
#             4012 POP_TOP
# 
# 23507:     >> 4014 LOAD_GLOBAL          (port)
#             4016 LOAD_ATTR            (On_Effect)
#             4018 LOAD_CONST           (0)
#             4020 BINARY_SUBSCR
#             4022 LOAD_CONST           (255)
#             4024 COMPARE_OP           (==)
#             4026 EXTENDED_ARG         (4096)
#             4028 POP_JUMP_IF_FALSE    (to 4188)
# 
# 23510:        4030 LOAD_FAST            (self)
#             4032 LOAD_ATTR            (liusu_fan_lable)
#             4034 LOAD_METHOD          (hide)
#             4036 CALL_METHOD          (0 positional arguments)
#             4038 POP_TOP
# 
# 23511:        4040 LOAD_FAST            (self)
#             4042 LOAD_ATTR            (slider_fan_liusu)
#             4044 LOAD_METHOD          (hide)
#             4046 CALL_METHOD          (0 positional arguments)
#             4048 POP_TOP
# 
# 23512:        4050 LOAD_FAST            (self)
#             4052 LOAD_ATTR            (liusu_fan_text_lable)
#             4054 LOAD_METHOD          (hide)
#             4056 CALL_METHOD          (0 positional arguments)
#             4058 POP_TOP
# 
# 23514:        4060 LOAD_FAST            (self)
#             4062 LOAD_ATTR            (fangxiang_fan_lable)
#             4064 LOAD_METHOD          (hide)
#             4066 CALL_METHOD          (0 positional arguments)
#             4068 POP_TOP
# 
# 23515:        4070 LOAD_FAST            (self)
#             4072 LOAD_ATTR            (fan_fangxiang_buttons)
#             4074 GET_ITER
#          >> 4076 FOR_ITER             (to 4092)
#             4078 STORE_FAST           (i)
# 
# 23516:        4080 LOAD_FAST            (i)
#             4082 LOAD_METHOD          (hide)
#             4084 CALL_METHOD          (0 positional arguments)
#             4086 POP_TOP
#             4088 EXTENDED_ARG         (3840)
#             4090 JUMP_ABSOLUTE        (to 4076)
# 
# 23518:     >> 4092 LOAD_FAST            (self)
#             4094 LOAD_ATTR            (liangdu_fan_lable)
#             4096 LOAD_METHOD          (hide)
#             4098 CALL_METHOD          (0 positional arguments)
#             4100 POP_TOP
# 
# 23519:        4102 LOAD_FAST            (self)
#             4104 LOAD_ATTR            (slider_fan_liangdu)
#             4106 LOAD_METHOD          (hide)
#             4108 CALL_METHOD          (0 positional arguments)
#             4110 POP_TOP
# 
# 23520:        4112 LOAD_FAST            (self)
#             4114 LOAD_ATTR            (liangdu_fan_text_lable)
#             4116 LOAD_METHOD          (hide)
#             4118 CALL_METHOD          (0 positional arguments)
#             4120 POP_TOP
# 
# 23522:        4122 LOAD_FAST            (self)
#             4124 LOAD_ATTR            (yanse_fan_lable)
#             4126 LOAD_METHOD          (hide)
#             4128 CALL_METHOD          (0 positional arguments)
#             4130 POP_TOP
# 
# 23523:        4132 LOAD_FAST            (self)
#             4134 LOAD_ATTR            (fan_dengxiao_buttons)
#             4136 GET_ITER
#          >> 4138 FOR_ITER             (to 4154)
#             4140 STORE_FAST           (i)
# 
# 23524:        4142 LOAD_FAST            (i)
#             4144 LOAD_METHOD          (hide)
#             4146 CALL_METHOD          (0 positional arguments)
#             4148 POP_TOP
#             4150 EXTENDED_ARG         (4096)
#             4152 JUMP_ABSOLUTE        (to 4138)
# 
# 23525:     >> 4154 LOAD_FAST            (self)
#             4156 LOAD_ATTR            (button_fan_tiaose)
#             4158 LOAD_METHOD          (hide)
#             4160 CALL_METHOD          (0 positional arguments)
#             4162 POP_TOP
# 
# 23526:        4164 LOAD_FAST            (self)
#             4166 LOAD_ATTR            (jiyi_changer_switch)
#             4168 LOAD_METHOD          (setChecked)
#             4170 LOAD_CONST           (True)
#             4172 CALL_METHOD          (1 positional argument)
#             4174 POP_TOP
# 
# 23527:        4176 LOAD_FAST            (self)
#             4178 LOAD_ATTR            (guding_changer_switch)
#             4180 LOAD_METHOD          (setChecked)
#             4182 LOAD_CONST           (False)
#             4184 CALL_METHOD          (1 positional argument)
#             4186 POP_TOP
# 
# 23528:     >> 4188 LOAD_FAST            (self)
#             4190 LOAD_ATTR            (effect_list_label)
#             4192 LOAD_METHOD          (show)
#             4194 CALL_METHOD          (0 positional arguments)
#             4196 POP_TOP
# 
# 23529:        4198 LOAD_FAST            (self)
#             4200 LOAD_ATTR            (fan_effect_list)
#             4202 LOAD_METHOD          (hide)
#             4204 CALL_METHOD          (0 positional arguments)
#             4206 POP_TOP
# 
# 23530:        4208 LOAD_FAST            (self)
#             4210 LOAD_ATTR            (ARGB_FAN_sync_all)
#             4212 LOAD_METHOD          (hide)
#             4214 CALL_METHOD          (0 positional arguments)
#             4216 POP_TOP
# 
# 23531:        4218 LOAD_FAST            (self)
#             4220 LOAD_ATTR            (ARGB_FAN_all)
#             4222 LOAD_METHOD          (hide)
#             4224 CALL_METHOD          (0 positional arguments)
#             4226 POP_TOP
# 
# 23532:        4228 LOAD_GLOBAL          (range)
#             4230 LOAD_CONST           (6)
#             4232 CALL_FUNCTION        (1 positional argument)
#             4234 GET_ITER
#          >> 4236 FOR_ITER             (to 4258)
#             4238 STORE_FAST           (i)
# 
# 23533:        4240 LOAD_FAST            (self)
#             4242 LOAD_ATTR            (ARGB_FAN_chuan)
#             4244 LOAD_FAST            (i)
#             4246 BINARY_SUBSCR
#             4248 LOAD_METHOD          (hide)
#             4250 CALL_METHOD          (0 positional arguments)
#             4252 POP_TOP
#             4254 EXTENDED_ARG         (4096)
#             4256 JUMP_ABSOLUTE        (to 4236)
# 
# 23535:     >> 4258 LOAD_FAST            (self)
#             4260 LOAD_ATTR            (chuji_effect_widget)
#             4262 LOAD_METHOD          (hide)
#             4264 CALL_METHOD          (0 positional arguments)
#             4266 POP_TOP
# 
# 23536:        4268 LOAD_GLOBAL          (range)
#             4270 LOAD_CONST           (10)
#             4272 CALL_FUNCTION        (1 positional argument)
#             4274 GET_ITER
#          >> 4276 FOR_ITER             (to 4456)
#             4278 STORE_FAST           (i)
# 
# 23537:        4280 LOAD_FAST            (self)
#             4282 LOAD_ATTR            (ARGB_FAN_devices)
#             4284 LOAD_FAST            (i)
#             4286 BINARY_SUBSCR
#             4288 LOAD_METHOD          (show)
#             4290 CALL_METHOD          (0 positional arguments)
#             4292 POP_TOP
# 
# 23538:        4294 LOAD_FAST            (self)
#             4296 LOAD_ATTR            (ARGB_FAN_SN_NUM)
#             4298 LOAD_FAST            (i)
#             4300 BINARY_SUBSCR
#             4302 LOAD_METHOD          (show)
#             4304 CALL_METHOD          (0 positional arguments)
#             4306 POP_TOP
# 
# 23539:        4308 LOAD_FAST            (i)
#             4310 LOAD_CONST           (0)
#             4312 COMPARE_OP           (==)
#             4314 EXTENDED_ARG         (4352)
#             4316 POP_JUMP_IF_FALSE    (to 4358)
# 
# 23540:        4318 LOAD_FAST            (self)
#             4320 LOAD_ATTR            (ARGB_FAN_devices)
#             4322 LOAD_FAST            (i)
#             4324 BINARY_SUBSCR
#             4326 LOAD_METHOD          (setIcon)
# 
# 23541:        4328 LOAD_GLOBAL          (QIcon)
#             4330 LOAD_GLOBAL          (port)
#             4332 LOAD_METHOD          (file_path_img)
#             4334 LOAD_GLOBAL          (port)
#             4336 LOAD_METHOD          (file_path_img)
#             4338 LOAD_FAST            (self)
#             4340 LOAD_ATTR            (ARGB_FAN_devices_img_xuanzhong)
#             4342 LOAD_FAST            (i)
#             4344 BINARY_SUBSCR
#             4346 CALL_METHOD          (1 positional argument)
#             4348 CALL_METHOD          (1 positional argument)
#             4350 CALL_FUNCTION        (1 positional argument)
# 
# 23540:        4352 CALL_METHOD          (1 positional argument)
#             4354 POP_TOP
#             4356 JUMP_FORWARD         (to 4452)
# 
# 23543:     >> 4358 LOAD_GLOBAL          (port)
#             4360 LOAD_ATTR            (ARGB_TEN_NUM)
#             4362 LOAD_FAST            (i)
#             4364 BINARY_SUBSCR
#             4366 LOAD_CONST           (0)
#             4368 COMPARE_OP           (==)
#             4370 EXTENDED_ARG         (4352)
#             4372 POP_JUMP_IF_FALSE    (to 4414)
# 
# 23544:        4374 LOAD_FAST            (self)
#             4376 LOAD_ATTR            (ARGB_FAN_devices)
#             4378 LOAD_FAST            (i)
#             4380 BINARY_SUBSCR
#             4382 LOAD_METHOD          (setIcon)
#             4384 LOAD_GLOBAL          (QIcon)
#             4386 LOAD_GLOBAL          (port)
#             4388 LOAD_METHOD          (file_path_img)
#             4390 LOAD_GLOBAL          (port)
#             4392 LOAD_METHOD          (file_path_img)
#             4394 LOAD_FAST            (self)
#             4396 LOAD_ATTR            (ARGB_FAN_devices_img_none)
#             4398 LOAD_FAST            (i)
#             4400 BINARY_SUBSCR
#             4402 CALL_METHOD          (1 positional argument)
#             4404 CALL_METHOD          (1 positional argument)
#             4406 CALL_FUNCTION        (1 positional argument)
#             4408 CALL_METHOD          (1 positional argument)
#             4410 POP_TOP
#             4412 JUMP_FORWARD         (to 4452)
# 
# 23546:     >> 4414 LOAD_FAST            (self)
#             4416 LOAD_ATTR            (ARGB_FAN_devices)
#             4418 LOAD_FAST            (i)
#             4420 BINARY_SUBSCR
#             4422 LOAD_METHOD          (setIcon)
#             4424 LOAD_GLOBAL          (QIcon)
#             4426 LOAD_GLOBAL          (port)
#             4428 LOAD_METHOD          (file_path_img)
#             4430 LOAD_GLOBAL          (port)
#             4432 LOAD_METHOD          (file_path_img)
#             4434 LOAD_FAST            (self)
#             4436 LOAD_ATTR            (ARGB_FAN_devices_img)
#             4438 LOAD_FAST            (i)
#             4440 BINARY_SUBSCR
#             4442 CALL_METHOD          (1 positional argument)
#             4444 CALL_METHOD          (1 positional argument)
#             4446 CALL_FUNCTION        (1 positional argument)
#             4448 CALL_METHOD          (1 positional argument)
#             4450 POP_TOP
#          >> 4452 EXTENDED_ARG         (4096)
#             4454 JUMP_ABSOLUTE        (to 4276)
# 
# 23547:     >> 4456 LOAD_GLOBAL          (agreement)
#             4458 LOAD_METHOD          (argb_fan_set)
#             4460 LOAD_GLOBAL          (port)
#             4462 LOAD_ATTR            (ARGB_ser)
#             4464 LOAD_CONST           (0)
#             4466 CALL_METHOD          (2 positional arguments)
#             4468 POP_TOP
# 
# 23548:        4470 LOAD_FAST            (self)
#             4472 LOAD_METHOD          (fan_set_config)
#             4474 LOAD_CONST           (0)
#             4476 CALL_METHOD          (1 positional argument)
#             4478 POP_TOP
# 
# 23549:        4480 LOAD_GLOBAL          (port)
#             4482 LOAD_ATTR            (ARGB_433RF)
#             4484 LOAD_CONST           (None)
#             4486 COMPARE_OP           (is-not)
#             4488 EXTENDED_ARG         (4608)
#             4490 POP_JUMP_IF_FALSE    (to 4710)
# 
# 23550:        4492 LOAD_GLOBAL          (port)
#             4494 LOAD_ATTR            (ARGB_433RF)
#             4496 LOAD_CONST           (0)
#             4498 BINARY_SUBSCR
#             4500 LOAD_CONST           (1)
#             4502 COMPARE_OP           (==)
#             4504 EXTENDED_ARG         (4608)
#             4506 POP_JUMP_IF_FALSE    (to 4644)
# 
# 23551:        4508 LOAD_GLOBAL          (port)
#             4510 LOAD_ATTR            (language)
#             4512 LOAD_CONST           ("zh")
#             4514 COMPARE_OP           (==)
#             4516 EXTENDED_ARG         (4352)
#             4518 POP_JUMP_IF_FALSE    (to 4568)
# 
# 23552:        4520 LOAD_FAST            (self)
#             4522 LOAD_ATTR            (ARGB_rf_lable)
#             4524 LOAD_METHOD          (setText)
#             4526 LOAD_CONST           ("遥控器配对状态: <span style='color:red;'>已配对</span>  配对码信息： ")
#             4528 LOAD_CONST           ("")
#             4530 LOAD_METHOD          (join)
#             4532 LOAD_CONST           (<Code38 code object <listcomp> at 0x2589185a970, file Sync_light.py>, line 23552)
#             4534 LOAD_CONST           ("MainWindow.fan_dengxiao_index.<locals>.<listcomp>")
#             4536 MAKE_FUNCTION        (No arguments)
#             4538 LOAD_GLOBAL          (port)
#             4540 LOAD_ATTR            (ARGB_433RF)
#             4542 LOAD_CONST           (1)
#             4544 LOAD_CONST           (3)
#             4546 BUILD_SLICE          2
#             4548 BINARY_SUBSCR
#             4550 GET_ITER
#             4552 CALL_FUNCTION        (1 positional argument)
#             4554 CALL_METHOD          (1 positional argument)
#             4556 FORMAT_VALUE         0
#             4558 LOAD_CONST           ("  ")
#             4560 BUILD_STRING         3
#             4562 CALL_METHOD          (1 positional argument)
#             4564 POP_TOP
#             4566 JUMP_FORWARD         (to 4614)
# 
# 23554:     >> 4568 LOAD_FAST            (self)
#             4570 LOAD_ATTR            (ARGB_rf_lable)
#             4572 LOAD_METHOD          (setText)
#             4574 LOAD_CONST           ("Remote pairing status: <span style='color:red;'>Paired</span>  Pairing code information: ")
#             4576 LOAD_CONST           ("")
#             4578 LOAD_METHOD          (join)
#             4580 LOAD_CONST           (<Code38 code object <listcomp> at 0x2589185aa50, file Sync_light.py>, line 23554)
#             4582 LOAD_CONST           ("MainWindow.fan_dengxiao_index.<locals>.<listcomp>")
#             4584 MAKE_FUNCTION        (No arguments)
#             4586 LOAD_GLOBAL          (port)
#             4588 LOAD_ATTR            (ARGB_433RF)
#             4590 LOAD_CONST           (1)
#             4592 LOAD_CONST           (3)
#             4594 BUILD_SLICE          2
#             4596 BINARY_SUBSCR
#             4598 GET_ITER
#             4600 CALL_FUNCTION        (1 positional argument)
#             4602 CALL_METHOD          (1 positional argument)
#             4604 FORMAT_VALUE         0
#             4606 LOAD_CONST           ("  ")
#             4608 BUILD_STRING         3
#             4610 CALL_METHOD          (1 positional argument)
#             4612 POP_TOP
# 
# 23555:     >> 4614 LOAD_FAST            (self)
#             4616 LOAD_ATTR            (rf_433_buttons)
#             4618 LOAD_CONST           (0)
#             4620 BINARY_SUBSCR
#             4622 LOAD_METHOD          (hide)
#             4624 CALL_METHOD          (0 positional arguments)
#             4626 POP_TOP
# 
# 23556:        4628 LOAD_FAST            (self)
#             4630 LOAD_ATTR            (rf_433_buttons)
#             4632 LOAD_CONST           (1)
#             4634 BINARY_SUBSCR
#             4636 LOAD_METHOD          (show)
#             4638 CALL_METHOD          (0 positional arguments)
#             4640 POP_TOP
#             4642 JUMP_FORWARD         (to 4710)
# 
# 23558:     >> 4644 LOAD_GLOBAL          (port)
#             4646 LOAD_ATTR            (language)
#             4648 LOAD_CONST           ("zh")
#             4650 COMPARE_OP           (==)
#             4652 EXTENDED_ARG         (4608)
#             4654 POP_JUMP_IF_FALSE    (to 4670)
# 
# 23560:        4656 LOAD_FAST            (self)
#             4658 LOAD_ATTR            (ARGB_rf_lable)
#             4660 LOAD_METHOD          (setText)
#             4662 LOAD_CONST           ("遥控器配对状态: 未配对  配对码信息：      ")
#             4664 CALL_METHOD          (1 positional argument)
#             4666 POP_TOP
#             4668 JUMP_FORWARD         (to 4682)
# 
# 23562:     >> 4670 LOAD_FAST            (self)
#             4672 LOAD_ATTR            (ARGB_rf_lable)
#             4674 LOAD_METHOD          (setText)
#             4676 LOAD_CONST           ("Remote pairing status: Unpaired  Pairing code information:      ")
#             4678 CALL_METHOD          (1 positional argument)
#             4680 POP_TOP
# 
# 23563:     >> 4682 LOAD_FAST            (self)
#             4684 LOAD_ATTR            (rf_433_buttons)
#             4686 LOAD_CONST           (1)
#             4688 BINARY_SUBSCR
#             4690 LOAD_METHOD          (hide)
#             4692 CALL_METHOD          (0 positional arguments)
#             4694 POP_TOP
# 
# 23564:        4696 LOAD_FAST            (self)
#             4698 LOAD_ATTR            (rf_433_buttons)
#             4700 LOAD_CONST           (0)
#             4702 BINARY_SUBSCR
#             4704 LOAD_METHOD          (show)
#             4706 CALL_METHOD          (0 positional arguments)
#             4708 POP_TOP
# 
# 23565:     >> 4710 LOAD_GLOBAL          (range)
#             4712 LOAD_CONST           (2)
#             4714 CALL_FUNCTION        (1 positional argument)
#             4716 GET_ITER
#          >> 4718 FOR_ITER             (to 4754)
#             4720 STORE_FAST           (i)
# 
# 23566:        4722 LOAD_FAST            (self)
#             4724 LOAD_ATTR            (fan_buttons_tese)
#             4726 LOAD_FAST            (i)
#             4728 BINARY_SUBSCR
#             4730 LOAD_METHOD          (hide)
#             4732 CALL_METHOD          (0 positional arguments)
#             4734 POP_TOP
# 
# 23567:        4736 LOAD_FAST            (self)
#             4738 LOAD_ATTR            (fan_lable_tese)
#             4740 LOAD_FAST            (i)
#             4742 BINARY_SUBSCR
#             4744 LOAD_METHOD          (hide)
#             4746 CALL_METHOD          (0 positional arguments)
#             4748 POP_TOP
#             4750 EXTENDED_ARG         (4608)
#             4752 JUMP_ABSOLUTE        (to 4718)
#          >> 4754 LOAD_CONST           (None)
#             4756 RETURN_VALUE
# 
# 
# # Method Name:       <listcomp>
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  2
# # Stack size:        3
# # Flags:             0x00000053 (NOFREE | NESTED | NEWLOCALS | OPTIMIZED)
# # First Line:        23042
# # Constants:
# #    0: 0
# # Varnames:
# #	.0, _
# # Positional arguments:
# #	.0
# # Local variables:
# #    1: _
# 23042:           0 BUILD_LIST           0
#                2 LOAD_FAST            (.0)
#          >>    4 FOR_ITER             (to 14)
# 
# 23042:           6 STORE_FAST           (_)
# 
# 23042:           8 LOAD_CONST           (0)
#               10 LIST_APPEND          2
#               12 JUMP_ABSOLUTE        (to 4)
#          >>   14 RETURN_VALUE
# 
# 
# # Method Name:       <lambda>
# # Filename:          Sync_light.py
# # Argument count:    0
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  0
# # Stack size:        2
# # Flags:             0x00000053 (NOFREE | NESTED | NEWLOCALS | OPTIMIZED)
# # First Line:        23340
# # Constants:
# #    0: None
# # Names:
# #    0: test
# #    1: ARGB_FAN_sound
# 23340:           0 LOAD_GLOBAL          (test)
#                2 LOAD_METHOD          (ARGB_FAN_sound)
#                4 CALL_METHOD          (0 positional arguments)
#                6 RETURN_VALUE
# 
# 
# # Method Name:       <listcomp>
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  2
# # Stack size:        3
# # Flags:             0x00000053 (NOFREE | NESTED | NEWLOCALS | OPTIMIZED)
# # First Line:        23503
# # Constants:
# #    0: 0
# # Varnames:
# #	.0, i
# # Positional arguments:
# #	.0
# # Local variables:
# #    1: i
# 23503:           0 BUILD_LIST           0
#                2 LOAD_FAST            (.0)
#          >>    4 FOR_ITER             (to 14)
# 
# 23503:           6 STORE_FAST           (i)
# 
# 23503:           8 LOAD_CONST           (0)
#               10 LIST_APPEND          2
#               12 JUMP_ABSOLUTE        (to 4)
#          >>   14 RETURN_VALUE
# 
# 
# # Method Name:       <listcomp>
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  2
# # Stack size:        4
# # Flags:             0x00000053 (NOFREE | NESTED | NEWLOCALS | OPTIMIZED)
# # First Line:        23552
# # Names:
# #    0: str
# # Varnames:
# #	.0, x
# # Positional arguments:
# #	.0
# # Local variables:
# #    1: x
# 23552:           0 BUILD_LIST           0
#                2 LOAD_FAST            (.0)
#          >>    4 FOR_ITER             (to 18)
# 
# 23552:           6 STORE_FAST           (x)
# 
# 23552:           8 LOAD_GLOBAL          (str)
#               10 LOAD_FAST            (x)
#               12 CALL_FUNCTION        (1 positional argument)
#               14 LIST_APPEND          2
#               16 JUMP_ABSOLUTE        (to 4)
#          >>   18 RETURN_VALUE
# 
# 
# # Method Name:       <listcomp>
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  2
# # Stack size:        4
# # Flags:             0x00000053 (NOFREE | NESTED | NEWLOCALS | OPTIMIZED)
# # First Line:        23554
# # Names:
# #    0: str
# # Varnames:
# #	.0, x
# # Positional arguments:
# #	.0
# # Local variables:
# #    1: x
# 23554:           0 BUILD_LIST           0
#                2 LOAD_FAST            (.0)
#          >>    4 FOR_ITER             (to 18)
# 
# 23554:           6 STORE_FAST           (x)
# 
# 23554:           8 LOAD_GLOBAL          (str)
#               10 LOAD_FAST            (x)
#               12 CALL_FUNCTION        (1 positional argument)
#               14 LIST_APPEND          2
#               16 JUMP_ABSOLUTE        (to 4)
#          >>   18 RETURN_VALUE
# 
# 
# ==== .MainWindow.update_color (linha 25181)
if len(port.usb_device_sn) == 0:
    self.tanchuang_no_device()
else:
    self.slider_huancai_speed.hide()
    self.slider_shiyin_speed.hide()
    for i, btn in enumerate(self.shiyin_buttons):
        btn.setChecked(False)
        self.shiyin_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.shiyin_img[i]))))

    for i, btn in enumerate(self.huancai_buttons):
        btn.setChecked(False)
        self.huancai_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.huancai_img[i]))))

    for i, btn in enumerate(self.buttons):
        btn.setChecked(False)
        btn.setStyleSheet(f"background-color: rgb{self.colors[i]}; border-radius: 16px;")

    self.button_onoff.setIcon(QIcon(port.file_path_img("img/light-on.svg")))
    self.button_onoff.setCheckable(True)
    self.button_tongping.setCheckable(True)
    self.button_tongping.setIcon(QIcon(port.file_path_img("img/fantasy1.svg")))
    self.button_charge = False
    red = self.slider_color.value()
    color = QColor()
    color.setHsv(red, 255, 255)
    t = test.ya_rgb_one(test.hex_to_rgb(color.name()))
    print("设定颜色", t)
    self.slider_color.setStyleSheet("\n            QSlider::groove:horizontal {\n                border: 1px solid #999999;\n                height: 16px;\n                background: qlineargradient(x1:0, y1:0, x2:1, y2:0, \n                                            stop:0 rgba(255, 0, 0, 255), \n                                            stop:0.17 rgba(255, 255, 0, 255), \n                                            stop:0.33 rgba(0, 255, 0, 255), \n                                            stop:0.5 rgba(0, 255, 255, 255), \n                                            stop:0.67 rgba(0, 0, 255, 255), \n                                            stop:0.83 rgba(255, 0, 255, 255), \n                                            stop:1 rgba(255, 0, 0, 255));\n                border-radius: 10px\n            }\n\n            QSlider::handle:horizontal {\n                background: %s;\n                border: 3px solid #FFF;\n                width: 16px;\n                height: 16px;\n                margin: -1px 0;\n                border-radius: 10px;\n                }\n            " % color.name())
    try:
        if self.devices_qun_button == 1:
            test.quse_exit_event_dongle.set()
            test.tongping_event_list = []
            port.devices_qun[1] = red
            port.devices_qun[0] = 1
            port.devices_qun[2] = 0
            port.devices_qun[4] = 0
            port.devices_qun[5] = 0
            port.devices_qun[6] = 0
            for i in range(len(port.usb_device_sn)):
                port.devices_ten[i][1] = red
                port.devices_ten[i][0] = 1
                port.devices_ten[i][2] = 0
                port.devices_ten[i][4] = 0
                port.devices_ten[i][5] = 0
                port.devices_ten[i][6] = 0
                port.devices_ten_color[i] = t
                test.device_dan_events[i].set()

            def color_qun(t):
                time.sleep(0.1)
                port.devices_qun_color = t
                if "dongle" in port.device_info_robobloq:
                    test.NO_tongping(port.ser_list[agreement.find_first_index(port.device_info_robobloq, "dongle")], [
                     255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1], t[2]])
                    test.tongping_none = [255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1], t[2]]
                for i in range(len(port.usb_device_sn)):
                    if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
                        test.NO_tongping(port.ser_list[i], [255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1], t[2]])
                        test.device_dan_no_tongping[i] = [3, t[0], t[1], t[2]]
                    if port.device_info_robobloq[i] == "usb_hid":
                        agreement.usb_hid_monochrome(t, port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])


            color_list = t
            t = threading.Thread(target=(lambda: color_qun(color_list)), daemon=True)
            t.start()
        else:
            port.devices_ten[self.device_dan_button - 1][1] = red
            port.devices_ten[self.device_dan_button - 1][0] = 1
            port.devices_ten[self.device_dan_button - 1][2] = 0
            port.devices_ten[self.device_dan_button - 1][4] = 0
            port.devices_ten[self.device_dan_button - 1][5] = 0
            port.devices_ten[self.device_dan_button - 1][6] = 0
            port.devices_ten_color[self.device_dan_button - 1] = t
            test.device_dan_events[self.device_dan_button - 1].set()
            self.panduan_tongping()
            index_button = self.device_dan_button - 1

            def color_dan(t, index):
                time.sleep(0.1)
                try:
                    if port.device_info_robobloq[index] == "dongle":
                        if test.thread_alive("dongle"):
                            test.tongping_none = port.usb_device_sn[index][0:6] + [3, t[0], t[1], t[2], 255, 255, 255, 255]
                        else:
                            test.NO_tongping(port.ser_list[index], port.usb_device_sn[index][0:6] + [3, t[0], t[1], t[2], 255, 255, 255, 255])
                    if port.device_info_robobloq[index] == "usb" or port.device_info_robobloq[index] == "sound":
                        test.NO_tongping(port.ser_list[self.device_dan_button - 1], [
                         255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1],
                         t[2]])
                        test.device_dan_no_tongping[self.device_dan_button - 1] = [3, t[0], t[1], t[2]]
                    if port.device_info_robobloq[index] == "usb_hid":
                        agreement.usb_hid_monochrome(t, port.ser_list[index], port.usb_device_rgbnum[index], port.usb_device_RGB[index])
                except:
                    pass


            color_list = t
            t = threading.Thread(target=(lambda: color_dan(color_list, index_button)), daemon=True)
            t.start()
    except Exception as e:
        try:
            print("颜色取色器报错：", e)
        finally:
            e = None
            del e

# global t ## Warning: Unused global
# ==== .MainWindow.update_color.color_qun (linha 25252)
time.sleep(0.1)
port.devices_qun_color = t
if "dongle" in port.device_info_robobloq:
    test.NO_tongping(port.ser_list[agreement.find_first_index(port.device_info_robobloq, "dongle")], [
     255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1], t[2]])
    test.tongping_none = [255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1], t[2]]
for i in range(len(port.usb_device_sn)):
    if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
        test.NO_tongping(port.ser_list[i], [255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1], t[2]])
        test.device_dan_no_tongping[i] = [3, t[0], t[1], t[2]]
    if port.device_info_robobloq[i] == "usb_hid":
        agreement.usb_hid_monochrome(t, port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])
# ==== .MainWindow.update_color.color_dan (linha 25280)
time.sleep(0.1)
try:
    if port.device_info_robobloq[index] == "dongle":
        if test.thread_alive("dongle"):
            test.tongping_none = port.usb_device_sn[index][0:6] + [3, t[0], t[1], t[2], 255, 255, 255, 255]
        else:
            test.NO_tongping(port.ser_list[index], port.usb_device_sn[index][0:6] + [3, t[0], t[1], t[2], 255, 255, 255, 255])
    if port.device_info_robobloq[index] == "usb" or port.device_info_robobloq[index] == "sound":
        test.NO_tongping(port.ser_list[self.device_dan_button - 1], [
         255,255,255,255,255,255] + [3, t[0], t[1], t[2], 3, t[0], t[1],
         t[2]])
        test.device_dan_no_tongping[self.device_dan_button - 1] = [3, t[0], t[1], t[2]]
    if port.device_info_robobloq[index] == "usb_hid":
        agreement.usb_hid_monochrome(t, port.ser_list[index], port.usb_device_rgbnum[index], port.usb_device_RGB[index])
except:
    pass
# ==== .MainWindow.fan_colors_changer (linha 25308)
red = self.fan_slider_color.value()
color = QColor()
color.setHsv(red, 255, 255)
t = test.ya_rgb_one(test.hex_to_rgb(color.name()))
index = self.fan_dengxiao_buttons.index(self.fan_sender_color)
self.fan_dengxiao_colors[index] = t
print(self.fan_dengxiao_colors)
absolute_path = port.file_path_img("img/duihao.png")
url = ""
for i in absolute_path:
    if i == "\\":
        url = url + "/"
    else:
        url = url + i

self.fan_dengxiao_buttons[index].setStyleSheet(f"QPushButton {{ background-image: url({url}); background-repeat: no-repeat; background-position: center; background-color: rgb{tuple(self.fan_dengxiao_colors[index])}; border-radius: 16px; }}")
print(f"风扇设定颜色 {t}, 选中的颜色：{index}")
if self.fan_sync_all == 1:
    agreement.ARGB_SYNC_yanse(port.ARGB_ser, self.fan_dengxiao_colors)
if self.fan_ten_stata != 0:
    for i in range(4):
        agreement.fan_order_list[i * 3] = self.fan_dengxiao_colors[i][0]
        agreement.fan_order_list[i * 3 + 1] = self.fan_dengxiao_colors[i][1]
        agreement.fan_order_list[i * 3 + 2] = self.fan_dengxiao_colors[i][2]

    agreement.ARGB_ten_effect(port.ARGB_ser, 4, self.fan_ten_stata - 1)
# ==== .MainWindow.update_color_diannao (linha 25337)
index_button = self.device_dan_button - 1
index_shiyin = port.devices_ten[index_button][6] - 1
red = self.slider_color_diannao.value()
color = QColor()
color.setHsv(red, 255, 255)
t = test.ya_rgb_one(test.hex_to_rgb(color.name()))
show_t = test.hex_to_rgb(color.name())
print("设定颜色", t)
self.slider_color_diannao.setStyleSheet("\n            QSlider::groove:horizontal {\n                border: 1px solid #999999;\n                height: 16px;\n                background: qlineargradient(x1:0, y1:0, x2:1, y2:0, \n                                            stop:0 rgba(255, 0, 0, 255), \n                                            stop:0.17 rgba(255, 255, 0, 255), \n                                            stop:0.33 rgba(0, 255, 0, 255), \n                                            stop:0.5 rgba(0, 255, 255, 255), \n                                            stop:0.67 rgba(0, 0, 255, 255), \n                                            stop:0.83 rgba(255, 0, 255, 255), \n                                            stop:1 rgba(255, 0, 0, 255));\n                border-radius: 10px\n            }\n\n            QSlider::handle:horizontal {\n                background: %s;\n                border: 3px solid #FFF;\n                width: 16px;\n                height: 16px;\n                margin: -1px 0;\n                border-radius: 10px;\n            }\n        " % color.name())
if self.diannao_shiyin_yanse_index == 1:
    self.button_diannao_colour1.setStyleSheet(f"background-color: rgb({show_t[0]}, {show_t[1]}, {show_t[2]}); color: white; border-radius: 16px; font-size: 16px;")
    port.diannao_shiyin_yanse[index_shiyin][0] = show_t
if self.diannao_shiyin_yanse_index == 2:
    self.button_diannao_colour2.setStyleSheet(f"background-color: rgb({show_t[0]}, {show_t[1]}, {show_t[2]}); color: white; border-radius: 16px; font-size: 16px;")
    port.diannao_shiyin_yanse[index_shiyin][1] = show_t
port.config_write_diannao_shiyin(port.diannao_shiyin_yanse)
print(f"电脑拾音保存颜色{port.diannao_shiyin_yanse}")
# ==== .MainWindow.light_changed (linha 25383)
if len(port.ser_list) == 0:
    self.tanchuang_no_device()
else:
    self.button_onoff.setIcon(QIcon(port.file_path_img("img/light-on.svg")))
    self.button_onoff.setCheckable(True)
    self.right_label.setText(str(self.slider.value()) + str("%"))
    if self.slider.value() <= 5:
        self.slider.setValue(5)
    light = int(self.slider.value() * 2.55)
    try:
        if self.devices_qun_button == 1:
            port.devices_qun[3] = self.slider.value()
            for i in range(len(port.usb_device_sn)):
                port.devices_ten[i][3] = self.slider.value()

            if "dongle" in port.device_info_robobloq:
                if test.thread_alive("dongle"):
                    test.tongping_none = [
                     255,255,255,255,255,255] + [2,light,255,255,2,light,255,255]
                else:
                    test.tongping_none = [
                     255,255,255,255,255,255] + [2,light,255,255,2,light,255,255]
                    test.NO_tongping(port.ser_list[agreement.find_first_index(port.device_info_robobloq, "dongle")], [
                     255,255,255,255,255,255] + [2,light,255,255,2,light,255,255])
            for i in range(len(port.usb_device_sn)):
                if port.device_info_robobloq[i] == "usb" or port.device_info_robobloq[i] == "sound":
                    test.device_dan_no_tongping[i] = [
                     2, light, 255, 255]
                    test.NO_tongping(port.ser_list[i], [255,255,255,255,255,255] + [2,light,255,255,2,light,255,255])
                if port.device_info_robobloq[i] == "usb_hid":
                    agreement.usb_hid_brightness_changer(port.ser_list[i], light)
                    if port.devices_ten[i][4:7] == [0, 0, 0]:
                        agreement.usb_hid_monochrome(port.devices_ten_color[i], port.ser_list[i], port.usb_device_rgbnum[i], port.usb_device_RGB[i])

        else:
            port.devices_ten[self.device_dan_button - 1][3] = self.slider.value()
            index_button = self.device_dan_button - 1
            if port.device_info_robobloq[index_button] == "dongle":
                if test.thread_alive("dongle"):
                    test.tongping_none = port.usb_device_sn[index_button][0:6] + [2,light,255,255,255,255,255,255]
                else:
                    test.tongping_none = port.usb_device_sn[index_button][0:6] + [2,light,255,255,255,255,255,255]
                    test.NO_tongping(port.ser_list[index_button], port.usb_device_sn[index_button][0:6] + [2,light,255,255,255,255,255,255])
            if port.device_info_robobloq[index_button] == "usb" or port.device_info_robobloq[index_button] == "sound":
                test.device_dan_no_tongping[index_button] = [
                 2, light, 255, 255]
                test.NO_tongping(port.ser_list[index_button], [255,255,255,255,255,255] + [2,light,255,255,2,light,255,255])
            if port.device_info_robobloq[index_button] == "usb_hid":
                agreement.usb_hid_brightness_changer(port.ser_list[index_button], light)
                if port.devices_ten[index_button][4:7] == [0, 0, 0]:
                    agreement.usb_hid_monochrome(port.devices_ten_color[index_button], port.ser_list[index_button], port.usb_device_rgbnum[index_button], port.usb_device_RGB[index_button])
    except Exception as e:
        try:
            print("亮度调节报错：", e)
        finally:
            e = None
            del e
# ==== .MainWindow.ARGB_send_show_effect (linha 25443)
agreement.fan_order_list = [0 for _ in range(52)]
dengxiaoleixing = 0
if port.fan_advanced_effect[2] == 1:
    dengxiaoleixing = 1
elif port.fan_advanced_effect[1] == 255:
    dengxiaoleixing = 0
elif port.fan_advanced_effect[1] < 10:
    dengxiaoleixing = 2
if dengxiaoleixing == 0:
    effect = self.fan_effect_list.currentIndex()
    fan_id = port.fan_advanced_effect[1]
    if fan_id != 255:
        for i in range(15):
            port.ARGB_dan_sync_config_gaoji[effect][i] = port.ARGB_TEN_MODEL1[fan_id][5][1 + i]

        agreement.fan_order_list[0] = effect
        agreement.fan_order_list[1] = port.ARGB_dan_sync_config_gaoji[effect][0]
        agreement.fan_order_list[2] = port.ARGB_dan_sync_config_gaoji[effect][1]
        agreement.fan_order_list[3] = port.ARGB_dan_sync_config_gaoji[effect][2]
        for i in range(12):
            agreement.fan_order_list[4 + i] = port.ARGB_dan_sync_config_gaoji[effect][3 + i]

        agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
    if dengxiaoleixing == 1:
        effect = port.fan_advanced_effect[0]
        agreement.fan_order_list[0] = self.anyc_list[effect]
        for i in range(15):
            agreement.fan_order_list[1 + i] = port.ARGB_qun_async_config[effect][i]

        agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
    if dengxiaoleixing == 2:
        fan_id = port.fan_advanced_effect[1]
        effect_leixing = [0, 0, 0]
        if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
            temp_leixing = port.gaoji_ten_dengxiao_leixing[fan_id]
            if temp_leixing[0] == 1:
                effect_leixing = [
                 0, 1, 5]
            elif temp_leixing[0] == 2:
                effect_leixing = [
                 0, 2, 5]
            elif temp_leixing[0] == 3:
                effect_leixing = [
                 0, 3, 4]
            elif temp_leixing[0] == 4:
                effect_leixing = [
                 5, 5, 5]
        else:
            effect_leixing = [
             5, 5, 5]
            port.gaoji_ten_dengxiao_leixing[fan_id][0] = 4
        agreement.fan_order_list[0] = port.gaoji_ten_dengxiao_leixing[fan_id][0]
        for i in range(6):
            agreement.fan_order_list[1 + i] = port.ARGB_TEN_MODEL1[fan_id][i][0]

        for i in range(3):
            t = effect_leixing[i]
            agreement.fan_order_list[7 + i] = port.ARGB_TEN_MODEL1[fan_id][t][1]
            agreement.fan_order_list[10 + i] = port.ARGB_TEN_MODEL1[fan_id][t][2]
            agreement.fan_order_list[13 + i] = port.ARGB_TEN_MODEL1[fan_id][t][3]

        for x in range(3):
            t = effect_leixing[x]
            index = 16 + 12 * x
            for i in range(12):
                agreement.fan_order_list[index + i] = port.ARGB_TEN_MODEL1[fan_id][t][4 + i]

        agreement.ARGB_ten_effect(port.ARGB_ser, 253, fan_id)
# ==== .MainWindow.fan_effect_list_changed (linha 25511)
if port.ARGB_ser is None:
    return
if test.fan_shiyin_event.is_set():
    pass
else:
    test.fan_shiyin_event.set()
    time.sleep(0.01)
effect = self.fan_effect_list.currentIndex()
async_id = port.fan_advanced_effect[2]
print(f"风扇灯效：{effect}")
port.fan_advanced_effect[0] = effect
port.argb_jiyi_write()
self.fan_effect_settings(effect)
print(f" port.fan_advanced_effect : {port.fan_advanced_effect}")
if port.fan_advanced_effect[2] == 2:
    port.fan_advanced_effect[1] = 255
    port.fan_advanced_effect[2] = 0
    self.tese_tongping_shiyin_xianshi(0)
if self.anyc_list[effect] != 255:
    self.ARGB_FAN_all.show()
    self.ARGB_FAN_sync_all.show()
else:
    if async_id == 1:
        port.fan_advanced_effect[2] = 0
        port.fan_advanced_effect[1] = 255
        self.ARGB_FAN_all.setStyleSheet("background-color: rgb(25,78,255);color: rgb(243,244,243);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
        self.ARGB_FAN_sync_all.setStyleSheet("background-color: rgb(243,244,243);color: rgb(51,51,50);font-size: 16px;border-radius: 10px;height: 40px;width: 135px;")
    self.ARGB_FAN_all.show()
    self.ARGB_FAN_sync_all.hide()
if port.fan_advanced_effect[2] != 1 and port.fan_advanced_effect[2] != 2:
    if port.fan_advanced_effect[1] != 255 and port.fan_advanced_effect[1] < 10:
        fan_id = port.fan_advanced_effect[1]
        fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
        if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
            if fan_effect_id != 5:
                self.ARGB_FAN_all.hide()
                self.ARGB_FAN_sync_all.hide()
            effect_id = [
             effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
            port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)
        else:
            effect_id = [
             effect] + port.ARGB_dan_config[5][effect][fan_id]
            port.ARGB_TEN_MODEL1[fan_id][5] = copy.deepcopy(effect_id)
        self.fan_effect_set_sync(effect_id)
    elif port.fan_advanced_effect[1] == 255:
        effect_id = [
         effect] + port.ARGB_dan_sync_config_gaoji[effect]
        self.fan_effect_set_sync(effect_id)
elif port.fan_advanced_effect[2] == 1:
    effect_id = [
     effect] + port.ARGB_qun_async_config[effect]
    self.fan_effect_set_sync(effect_id)
self.ARGB_send_show_effect()
port.argb_jiyi_write()
# ==== .MainWindow.fan_effect_set_sync (linha 25585)
print(f"灯效设置绑定数据: {fan_order_list}")
self.slider_fan_liusu.blockSignals(True)
for i in range(2):
    self.fan_fangxiang_buttons[i].blockSignals(True)

self.slider_fan_liangdu.blockSignals(True)
for i in range(4):
    self.fan_dengxiao_buttons[i].blockSignals(True)

speed = 60 - fan_order_list[1] * 10
self.slider_fan_liusu.setValue(speed)
if speed == 0:
    speed = 0.5
self.liusu_fan_text_lable.setText(str(int(speed * 2)) + "%")
if fan_order_list[2]:
    self.fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[0]))))
    self.fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[1]))))
    self.fan_fangxiang_buttons[0].setChecked(False)
    self.fan_fangxiang_buttons[1].setChecked(True)
else:
    self.fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[0]))))
    self.fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[1]))))
    self.fan_fangxiang_buttons[1].setChecked(False)
    self.fan_fangxiang_buttons[0].setChecked(True)
liangdu = fan_order_list[3] // 51 * 10
self.liangdu_fan_text_lable.setText(str(int(fan_order_list[3] / 2.55)) + "%")
if liangdu < 20:
    liangdu = 1
    self.liangdu_fan_text_lable.setText(str(1) + "%")
self.slider_fan_liangdu.setValue(liangdu)
rgb_list_12 = fan_order_list[4:16]
rgb_list_12_0 = rgb_list_12[0:3]
rgb_list_12_1 = rgb_list_12[3:6]
rgb_list_12_2 = rgb_list_12[6:9]
rgb_list_12_3 = rgb_list_12[9:12]
rgb_4 = [rgb_list_12_0, rgb_list_12_1, rgb_list_12_2, rgb_list_12_3]
for i in range(4):
    self.fan_dengxiao_buttons[i].setStyleSheet(f"background-color: rgb{tuple(rgb_4[i])}; border-radius: 20px;")
    self.fan_dengxiao_buttons[i].setChecked(False)

self.button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 16px;")
self.slider_fan_liusu.blockSignals(False)
for i in range(2):
    self.fan_fangxiang_buttons[i].blockSignals(False)

self.slider_fan_liangdu.blockSignals(False)
for i in range(4):
    self.fan_dengxiao_buttons[i].blockSignals(False)
# ==== .MainWindow.fan_effect_settings (linha 25644)
print(f"界面显示选项,{index}")
if port.ARGB_effect_check_list[index][1]:
    self.liusu_fan_lable.show()
    self.slider_fan_liusu.show()
    self.liusu_fan_text_lable.show()
else:
    self.liusu_fan_lable.hide()
    self.slider_fan_liusu.hide()
    self.liusu_fan_text_lable.hide()
if port.ARGB_effect_check_list[index][2]:
    self.fangxiang_fan_lable.show()
    for i in range(2):
        self.fan_fangxiang_buttons[i].show()

else:
    self.fangxiang_fan_lable.hide()
    for i in range(2):
        self.fan_fangxiang_buttons[i].hide()

if port.ARGB_effect_check_list[index][3]:
    self.liangdu_fan_lable.show()
    self.slider_fan_liangdu.show()
    self.liangdu_fan_text_lable.show()
else:
    self.liangdu_fan_lable.hide()
    self.slider_fan_liangdu.hide()
    self.liangdu_fan_text_lable.hide()
print(port.ARGB_effect_check_list[index][4])
if port.ARGB_effect_check_list[index][4]:
    self.yanse_fan_lable.show()
    self.button_fan_tiaose.show()
    for i in range(4):
        if i < port.ARGB_effect_check_list[index][4]:
            self.fan_dengxiao_buttons[i].show()
            print(i)
        else:
            self.fan_dengxiao_buttons[i].hide()

else:
    self.yanse_fan_lable.hide()
    self.button_fan_tiaose.hide()
    for i in range(4):
        self.fan_dengxiao_buttons[i].hide()
# ==== .MainWindow.fan_liusu_changed (linha 25692)
effect = self.fan_effect_list.currentIndex()
liusu = self.slider_fan_liusu.value()
fan_liusu = (60 - liusu) // 10
print(f"风扇流速：{fan_liusu}, {liusu}")
if liusu == 0:
    liusu = 0.5
self.liusu_fan_text_lable.setText(str(int(liusu * 2)) + "%")
if port.fan_advanced_effect[0] == 4:
    if port.On_Effect[0] == 1:
        agreement.fan_order_list = [0 for i in range(52)]
        port.On_Effect[2] = fan_liusu
        agreement.fan_order_list[0:16] = port.On_Effect[1:17]
        agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
port.write_argb_fan_all()
port.argb_jiyi_write()
# ==== .MainWindow.fan_liangdu_changed (linha 25738)
effect = self.fan_effect_list.currentIndex()
liangdu = self.slider_fan_liangdu.value()
fan_liangdu = liangdu // 10 * 51
self.liangdu_fan_text_lable.setText(str(int(fan_liangdu / 2.55)) + "%")
if fan_liangdu == 0:
    fan_liangdu = 50
    self.liangdu_fan_text_lable.setText(str(1) + "%")
if fan_liangdu != 0:
    print(f"风扇亮度：{fan_liangdu}, {liangdu}")
if port.fan_advanced_effect[0] == 4:
    if port.On_Effect[0] == 1:
        agreement.fan_order_list = [0 for i in range(52)]
        port.On_Effect[4] = fan_liangdu
        agreement.fan_order_list[0:16] = port.On_Effect[1:17]
        agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
port.write_argb_fan_all()
port.argb_jiyi_write()
# ==== .MainWindow.tongbu_effect_changer_Clicked (linha 26072)
sender = self.sender()
index = self.tongbu_effect_buttons.index(sender)
self.tongbu_effect_buttons[index].setChecked(True)
self.tongbu_effect_buttons[index].setIcon(QIcon(port.file_path_img(self.tongbu_effect_imgs[index])))
self.tongbu_effect_lables[index].setStyleSheet("color: rgb(242, 244, 241); font-size: 14px;")
port.fan_basic_effect = index
if sender.isChecked():
    print(f"Button {port.dengxiao_model2_zh[index]} is checked")
    for i, button in enumerate(self.tongbu_effect_buttons):
        if button is not sender:
            button.setChecked(False)
            self.tongbu_effect_buttons[i].setIcon(QIcon(port.file_path_img(self.tongbu_effect_imgs_xuanzhong[i])))
            self.tongbu_effect_lables[i].setStyleSheet("color: rgb(100, 100, 100); font-size: 14px;")

    if index == 0:
        print(port.ARGB_dan_sync_config_chuji[0])
        for i in range(3):
            port.ARGB_dan_sync_config_chuji[0][i * 3 + 6] = port.ARGB_dan_sync_config_chuji[0][3]
            port.ARGB_dan_sync_config_chuji[0][i * 3 + 7] = port.ARGB_dan_sync_config_chuji[0][4]
            port.ARGB_dan_sync_config_chuji[0][i * 3 + 8] = port.ARGB_dan_sync_config_chuji[0][5]

        agreement.fan_order_list[0] = index
        for i in range(15):
            agreement.fan_order_list[1 + i] = port.ARGB_dan_sync_config_chuji[index][i]

    if port.MCU_C_FLAG[0] == 0:
        if port.fan_advanced_effect[13] == 0:
            print(f"port.MCU_C_FLAG[0]:{port.MCU_C_FLAG[0]}")
            agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
self.tongbu_fan_effect_set_sync(agreement.fan_order_list)
self.tongbu_fan_effect_show_hide(index)
port.write_argb_fan_all()
port.argb_jiyi_write()
# ==== .MainWindow.tongbu_fan_effect_show_hide (linha 26106)
print(f"同步灯效设置界面显示选项,{index}")
if port.ARGB_effect_check_list[index][1]:
    self.tongbu_effect_liusu_group.show()
else:
    self.tongbu_effect_liusu_group.hide()
print(f"方向是否隐藏：{port.ARGB_effect_check_list[index][2]}")
if port.ARGB_effect_check_list[index][2]:
    self.tongbu_effect_fangxiang_group.show()
else:
    self.tongbu_effect_fangxiang_group.hide()
if port.ARGB_effect_check_list[index][3]:
    self.tongbu_effect_liangdu_group.show()
else:
    self.tongbu_effect_liangdu_group.hide()
if index == 0:
    port.ARGB_effect_check_list[index][4] = 1
if port.ARGB_effect_check_list[index][4]:
    self.tongbu_effect_yanse_group.show()
    for i in range(4):
        if i < port.ARGB_effect_check_list[index][4]:
            self.tongbu_fan_dengxiao_buttons[i].show()
        else:
            self.tongbu_fan_dengxiao_buttons[i].hide()

else:
    self.tongbu_effect_yanse_group.hide()
# ==== .MainWindow.tongbu_fan_effect_set_sync (linha 26138)
print(f"灯效设置绑定数据: {fan_order_list}")
self.tongbu_slider_liusu.blockSignals(True)
for i in range(2):
    self.tongbu_fan_fangxiang_buttons[i].blockSignals(True)

self.tongbu_slider_liangdu.blockSignals(True)
for i in range(4):
    self.tongbu_fan_dengxiao_buttons[i].blockSignals(True)

speed = 60 - fan_order_list[1] * 10
self.tongbu_slider_liusu.setValue(speed)
if speed == 0:
    speed = 0.5
self.tongbu_liusu_lable.setText(str(int(speed * 2)) + "%")
if fan_order_list[2]:
    self.tongbu_fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[0]))))
    self.tongbu_fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[1]))))
    self.tongbu_fan_fangxiang_buttons[0].setChecked(False)
    self.tongbu_fan_fangxiang_buttons[1].setChecked(True)
    self.tongbu_fangxiang_lable.setText(self.zheng_daoxu_flag[0])
else:
    self.tongbu_fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[0]))))
    self.tongbu_fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[1]))))
    self.tongbu_fan_fangxiang_buttons[1].setChecked(False)
    self.tongbu_fan_fangxiang_buttons[0].setChecked(True)
    self.tongbu_fangxiang_lable.setText(self.zheng_daoxu_flag[1])
liangdu = fan_order_list[3] // 51 * 10
self.tongbu_liangdu_lable.setText(str(int(fan_order_list[3] / 2.55)) + "%")
if liangdu < 20:
    liangdu = 1
    self.tongbu_liangdu_lable.setText(str(1) + "%")
self.tongbu_slider_liangdu.setValue(liangdu)
rgb_list_12 = fan_order_list[4:16]
rgb_list_12_0 = rgb_list_12[0:3]
rgb_list_12_1 = rgb_list_12[3:6]
rgb_list_12_2 = rgb_list_12[6:9]
rgb_list_12_3 = rgb_list_12[9:12]
rgb_4 = [rgb_list_12_0, rgb_list_12_1, rgb_list_12_2, rgb_list_12_3]
for i in range(4):
    self.tongbu_fan_dengxiao_buttons[i].setStyleSheet(f"background-color: rgb{tuple(rgb_4[i])}; border-radius: 20px;")
    self.tongbu_fan_dengxiao_buttons[i].setChecked(False)

self.tongbu_button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 16px;")
self.tongbu_slider_liusu.blockSignals(False)
for i in range(2):
    self.tongbu_fan_fangxiang_buttons[i].blockSignals(False)

self.tongbu_slider_liangdu.blockSignals(False)
for i in range(4):
    self.tongbu_fan_dengxiao_buttons[i].blockSignals(False)
# ==== .MainWindow.tongbu_tiaose_changer (linha 26201)
color_dialog = QColorDialog(self)
color_dialog.setOption(QColorDialog.ColorDialogOption.DontUseNativeDialog)
color = color_dialog.getColor()
if color.isValid():
    self.fan_tiaose_color = test.hex_to_rgb(color.name())
    print("选择颜色:", color.name(), self.fan_tiaose_color)
    self.tongbu_button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 16px;")
else:
    print("取消选择颜色")
    return
for i, btn in enumerate(self.tongbu_fan_dengxiao_buttons):
    btn.setChecked(False)
# ==== .MainWindow.tongbu_yanse_changer (linha 26218)
self.fan_sender_color = self.sender()
if self.fan_sender_color.isChecked():
    index = self.tongbu_fan_dengxiao_buttons.index(self.fan_sender_color)
    self.fan_sender_color.setStyleSheet(f"QPushButton {{ background-image: url(); background-repeat: no-repeat; background-position: center; background-color: rgb{tuple(self.fan_tiaose_color)}; border-radius: 20px; }}")
    print(f"Button {index} is checked")
    colors_four_list = [[0, 0, 0] for _ in range(4)]
    effect = port.fan_basic_effect
    agreement.fan_order_list[0] = effect
    port.ARGB_dan_sync_config_chuji[effect][3 + index * 3] = self.fan_tiaose_color[0]
    port.ARGB_dan_sync_config_chuji[effect][3 + index * 3 + 1] = self.fan_tiaose_color[1]
    port.ARGB_dan_sync_config_chuji[effect][3 + index * 3 + 2] = self.fan_tiaose_color[2]
    if effect == 0:
        for i in range(3):
            port.ARGB_dan_sync_config_chuji[0][i * 3 + 6] = port.ARGB_dan_sync_config_chuji[0][3]
            port.ARGB_dan_sync_config_chuji[0][i * 3 + 7] = port.ARGB_dan_sync_config_chuji[0][4]
            port.ARGB_dan_sync_config_chuji[0][i * 3 + 8] = port.ARGB_dan_sync_config_chuji[0][5]

        for index, vaule in enumerate(port.ARGB_dan_sync_config_chuji[effect]):
            agreement.fan_order_list[index + 1] = vaule

        agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
        for i in range(4):
            colors_four_list[i][0] = port.ARGB_dan_sync_config_chuji[effect][3 + i * 3]
            colors_four_list[i][1] = port.ARGB_dan_sync_config_chuji[effect][3 + i * 3 + 1]
            colors_four_list[i][2] = port.ARGB_dan_sync_config_chuji[effect][3 + i * 3 + 2]

        port.write_argb_fan_all()
        port.argb_jiyi_write()
        for button in self.tongbu_fan_dengxiao_buttons:
            if button is not self.fan_sender_color:
                button.setChecked(False)

    for i, btn in enumerate(self.tongbu_fan_dengxiao_buttons):
        try:
            if i != index:
                btn.setStyleSheet(f"background-color: rgb{tuple(colors_four_list[i])}; border-radius: 20px;")
        except:
            pass
# ==== .MainWindow.tongbu_effect_liusu_changer (linha 26257)
liusu = self.tongbu_slider_liusu.value()
fan_liusu = (60 - liusu) // 10
if liusu % 10 != 0:
    temp_vaule = 10
else:
    temp_vaule = 0
liusu = liusu // 10 * 10 + temp_vaule
self.tongbu_slider_liusu.setValue(liusu)
print(f"风扇流速：{fan_liusu}, {liusu}")
if liusu == 0:
    liusu = 0.5
self.tongbu_liusu_lable.setText(str(int(liusu * 2)) + "%")
effect = port.fan_basic_effect
agreement.fan_order_list[0] = effect
if port.ARGB_dan_sync_config_chuji[effect][0] != fan_liusu:
    port.ARGB_dan_sync_config_chuji[effect][0] = fan_liusu
    for index, vaule in enumerate(port.ARGB_dan_sync_config_chuji[effect]):
        agreement.fan_order_list[index + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.tongbu_effect_liangdu_changer (linha 26282)
liangdu = self.tongbu_slider_liangdu.value()
fan_liangdu = liangdu // 10 * 51
liangdu = liangdu // 10 * 10
self.tongbu_slider_liangdu.setValue(liangdu)
self.tongbu_liangdu_lable.setText(str(int(fan_liangdu / 2.55)) + "%")
if fan_liangdu == 0:
    fan_liangdu = 50
    self.tongbu_liangdu_lable.setText(str(1) + "%")
effect = port.fan_basic_effect
agreement.fan_order_list[0] = effect
if port.ARGB_dan_sync_config_chuji[effect][2] != fan_liangdu:
    port.ARGB_dan_sync_config_chuji[effect][2] = fan_liangdu
    for index, vaule in enumerate(port.ARGB_dan_sync_config_chuji[effect]):
        agreement.fan_order_list[index + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.tongbu_effect_fangxiang_changer (linha 26302)
fan_fangxiang_sender = self.sender()
if fan_fangxiang_sender.isChecked():
    index = self.tongbu_fan_fangxiang_buttons.index(fan_fangxiang_sender)
    print(f"Button {index} is checked")
    for button in self.tongbu_fan_fangxiang_buttons:
        if button is not fan_fangxiang_sender:
            button.setChecked(False)
        else:
            self.tongbu_fan_fangxiang_buttons[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[index]))))

    for i, btn in enumerate(self.tongbu_fan_fangxiang_buttons):
        if i != index:
            self.tongbu_fan_fangxiang_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[i]))))
        effect = port.fan_basic_effect
        agreement.fan_order_list[0] = effect
        port.ARGB_dan_sync_config_chuji[effect][1] = index

    for i, vaule in enumerate(port.ARGB_dan_sync_config_chuji[effect]):
        agreement.fan_order_list[i + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 255, 255)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
    if index:
        self.tongbu_fangxiang_lable.setText(self.zheng_daoxu_flag[0])
    else:
        self.tongbu_fangxiang_lable.setText(self.zheng_daoxu_flag[1])
# ==== .MainWindow.yibu_effect_send (linha 26331)
effect_leixing = [
 0, 0, 0]
if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
    temp_leixing = port.gaoji_ten_dengxiao_leixing[fan_id]
    if temp_leixing[0] == 1:
        effect_leixing = [
         0, 1, 5]
    elif temp_leixing[0] == 2:
        effect_leixing = [
         0, 2, 5]
    elif temp_leixing[0] == 3:
        effect_leixing = [
         0, 3, 4]
    elif temp_leixing[0] == 4:
        effect_leixing = [
         5, 5, 5]
else:
    effect_leixing = [
     5, 5, 5]
    port.gaoji_ten_dengxiao_leixing[fan_id][0] = 4
agreement.fan_order_list[0] = port.gaoji_ten_dengxiao_leixing[fan_id][0]
for i in range(6):
    agreement.fan_order_list[1 + i] = port.ARGB_TEN_MODEL1[fan_id][i][0]

for i in range(3):
    t = effect_leixing[i]
    agreement.fan_order_list[7 + i] = port.ARGB_TEN_MODEL1[fan_id][t][1]
    agreement.fan_order_list[10 + i] = port.ARGB_TEN_MODEL1[fan_id][t][2]
    agreement.fan_order_list[13 + i] = port.ARGB_TEN_MODEL1[fan_id][t][3]

for x in range(3):
    t = effect_leixing[x]
    index = 16 + 12 * x
    for i in range(12):
        agreement.fan_order_list[index + i] = port.ARGB_TEN_MODEL1[fan_id][t][4 + i]

if port.fan_advanced_effect[19][0] == 1:
    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        for i in range(10):
            jimu_fan_id = port.fan_advanced_effect[19][1]
            if port.ARGB_TEN_LEIXING[i] in port.jimu_lianpai:
                if i != jimu_fan_id:
                    port.ARGB_TEN_MODEL1[i] = copy.deepcopy(port.ARGB_TEN_MODEL1[jimu_fan_id])
                    port.gaoji_ten_dengxiao_leixing[i] = copy.deepcopy(port.gaoji_ten_dengxiao_leixing[jimu_fan_id])
                    for t in range(6):
                        effect_id = port.ARGB_TEN_MODEL1[jimu_fan_id][t][0]
                        port.ARGB_dan_config[t][effect_id][i] = copy.deepcopy(port.ARGB_dan_config[t][effect_id][jimu_fan_id])

                    agreement.ARGB_ten_effect(port.ARGB_ser, 253, i)
                port.write_argb_fan_all()
                port.argb_jiyi_write()

    else:
        agreement.ARGB_ten_effect(port.ARGB_ser, 253, fan_id)
else:
    agreement.ARGB_ten_effect(port.ARGB_ser, 253, fan_id)
# ==== .MainWindow.yibu_effect_baocun (linha 26380)
effect_leixing = [
 0, 0, 0]
if port.fan_advanced_effect[19][0] == 1:
    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        fan_id = copy.deepcopy(port.fan_advanced_effect[19][1])
    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        temp_leixing = port.gaoji_ten_dengxiao_leixing[fan_id]
        if temp_leixing[0] == 1:
            effect_leixing = [
             0, 1, 5]
        elif temp_leixing[0] == 2:
            effect_leixing = [
             0, 2, 5]
        elif temp_leixing[0] == 3:
            effect_leixing = [
             0, 3, 4]
        elif temp_leixing[0] == 4:
            effect_leixing = [
             5, 5, 5]
    else:
        effect_leixing = [
         5, 5, 5]
        port.gaoji_ten_dengxiao_leixing[fan_id][0] = 4
    agreement.fan_order_list[0] = port.gaoji_ten_dengxiao_leixing[fan_id][0]
    for i in range(6):
        agreement.fan_order_list[1 + i] = port.ARGB_TEN_MODEL1[fan_id][i][0]

    for i in range(3):
        t = effect_leixing[i]
        agreement.fan_order_list[7 + i] = port.ARGB_TEN_MODEL1[fan_id][t][1]
        agreement.fan_order_list[10 + i] = port.ARGB_TEN_MODEL1[fan_id][t][2]
        agreement.fan_order_list[13 + i] = port.ARGB_TEN_MODEL1[fan_id][t][3]

    for x in range(3):
        t = effect_leixing[x]
        index = 16 + 12 * x
        for i in range(12):
            agreement.fan_order_list[index + i] = port.ARGB_TEN_MODEL1[fan_id][t][4 + i]

    baocun_list = copy.deepcopy(agreement.fan_order_list)
    return baocun_list
# ==== .MainWindow.yibu_send_effect_Thread (linha 26417)
if port.MCU_C_FLAG[0] == 0 and port.fan_advanced_effect[13] == 0:
    if port.fan_advanced_effect[1] == 1:
        agreement.argb_fan_set(port.ARGB_ser, index)
        time.sleep(1)
    else:
        port.fan_advanced_effect[1] = 1
    for i in range(10):
        if port.ARGB_TEN_NUM[i] != 0:
            self.yibu_effect_send(i)
# ==== .MainWindow.yibu_fan_id_changer (linha 26429)
sender = self.sender()
index_fan_id = self.yibu_ten_fan_buttons.index(sender)
print(f"风扇 {index_fan_id} 选中")
self.yibu_ten_fan_buttons[index_fan_id].setChecked(True)
port.fan_advanced_effect[3] = index_fan_id
if port.PID_COMPAY == 5:
    self.yibu_ten_fan_buttons[index_fan_id].setIcon(QIcon(port.file_path_img(self.fan_tubiao_img_xuanzhong[index_fan_id])))
for index, vaule in enumerate(self.yibu_ten_fan_buttons):
    if index != index_fan_id:
        vaule.setChecked(False)
        vaule.setIcon(QIcon(port.file_path_img(self.fan_tubiao_img[index])))

if port.ARGB_TEN_LEIXING[index_fan_id] in port.jimu_lianpai:
    self.yibu_fan_jimu_widget.show()
    if port.ARGB_TEN_LEIXING[index_fan_id] != 9:
        self.yibu_six_fan_buttons[0].show()
        self.yibu_six_fan_buttons[1].show()
        self.yibu_six_fan_buttons[2].show()
        self.yibu_six_fan_buttons[3].show()
        self.yibu_six_fan_buttons[4].show()
        self.yibu_six_fan_buttons[5].show()
        print
    if port.ARGB_TEN_LEIXING[index_fan_id] == 9:
        self.yibu_six_fan_buttons[0].hide()
        self.yibu_six_fan_buttons[1].hide()
        self.yibu_six_fan_buttons[2].show()
        self.yibu_six_fan_buttons[3].show()
        self.yibu_six_fan_buttons[4].show()
        self.yibu_six_fan_buttons[5].show()
    if port.ARGB_TEN_LEIXING[index_fan_id] == 21:
        self.yibu_six_fan_buttons[0].hide()
        self.yibu_six_fan_buttons[1].hide()
        self.yibu_six_fan_buttons[2].hide()
        self.yibu_six_fan_buttons[3].hide()
        self.yibu_six_fan_buttons[4].hide()
        self.yibu_six_fan_buttons[5].hide()
    if port.fan_advanced_effect[19][0] == 1:
        self.yibu_six_fan_buttons[0].show()
        self.yibu_six_fan_buttons[1].show()
    if port.argb_fan_num():
        self.yibu_tongbu.show()
        self.yibu_tongbu_changer_switch.show()
    else:
        self.yibu_tongbu.hide()
        self.yibu_tongbu_changer_switch.hide()
    jimu_id = port.gaoji_ten_dengxiao_leixing[index_fan_id][2]
    print(jimu_id)
    if port.ARGB_TEN_LEIXING[index_fan_id] == 21:
        jimu_id = 5
    if port.ARGB_TEN_LEIXING[index_fan_id] == 9:
        if jimu_id < 2:
            jimu_id = 5
    self.yibu_six_fan_buttons[jimu_id].click()
else:
    jimu_id = 5
    self.yibu_fan_jimu_widget.hide()
effect = port.ARGB_TEN_MODEL1[index_fan_id][jimu_id][0]
self.yibu_fan_effect_show_hide(effect, index_fan_id)
self.yibu_effect_buttons[effect].click()
t = threading.Thread(target=(lambda: self.yibu_send_effect_Thread(index_fan_id)), daemon=True)
t.start()
port.write_argb_fan_all()
port.argb_jiyi_write()
# ==== .MainWindow.yibu_jimu_id_changer (linha 26504)
sender = self.sender()
index_jimu_id = self.yibu_six_fan_buttons.index(sender)
print(f"积木 {self.fan_six_text_zh[index_jimu_id]} 选中")
self.yibu_six_fan_buttons[index_jimu_id].setChecked(True)
for index, vaule in enumerate(self.yibu_six_fan_buttons):
    if index != index_jimu_id:
        vaule.setChecked(False)
    port.fan_advanced_effect[5] = index_jimu_id
    fan_id = port.fan_advanced_effect[3]
    port.gaoji_ten_dengxiao_leixing[fan_id][2] = copy.deepcopy(index_jimu_id)

if index_jimu_id == 0:
    port.gaoji_ten_dengxiao_leixing[fan_id][0] = copy.deepcopy(port.gaoji_ten_dengxiao_leixing[fan_id][1])
if index_jimu_id == 1:
    port.gaoji_ten_dengxiao_leixing[fan_id][0] = 1
    port.gaoji_ten_dengxiao_leixing[fan_id][1] = 1
if index_jimu_id == 2:
    port.gaoji_ten_dengxiao_leixing[fan_id][0] = 2
    port.gaoji_ten_dengxiao_leixing[fan_id][1] = 2
if index_jimu_id == 3 or index_jimu_id == 4:
    port.gaoji_ten_dengxiao_leixing[fan_id][0] = 3
    port.gaoji_ten_dengxiao_leixing[fan_id][1] = 3
if index_jimu_id == 5:
    port.gaoji_ten_dengxiao_leixing[fan_id][0] = 4
effect = port.ARGB_TEN_MODEL1[fan_id][index_jimu_id][0]
if port.MCU_C_FLAG[0] == 0:
    if port.fan_advanced_effect[13] == 0:
        self.yibu_effect_buttons[effect].click()
port.write_argb_fan_all()
port.argb_jiyi_write()
# ==== .MainWindow.yibu_effect_changer_Clicked (linha 26535)
sender = self.sender()
index = self.yibu_effect_buttons.index(sender)
self.yibu_effect_buttons[index].setChecked(True)
self.yibu_effect_buttons[index].setIcon(QIcon(port.file_path_img(self.yibu_effect_imgs[index])))
self.yibu_effect_lables[index].setStyleSheet("color: rgb(242, 244, 241); font-size: 14px; ")
port.fan_advanced_effect[4] = index
if sender.isChecked():
    print(f"Button {port.dengxiao_model2_zh[index]} is checked")
    for i, button in enumerate(self.yibu_effect_buttons):
        if button is not sender:
            button.setChecked(False)
            self.yibu_effect_buttons[i].setIcon(QIcon(port.file_path_img(self.yibu_effect_imgs_xuanzhong[i])))
            self.yibu_effect_lables[i].setStyleSheet("color: rgb(100, 100, 100); font-size: 14px; ")

fan_id = port.fan_advanced_effect[3]
fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
effect = index
if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
    effect_id = [
     effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
else:
    fan_effect_id = 5
    effect_id = [effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
if port.ARGB_HEX_SN == 0 and effect == 0:
    for i in range(3):
        effect_id[7 + i * 3] = effect_id[4]
        effect_id[8 + i * 3] = effect_id[5]
        effect_id[9 + i * 3] = effect_id[6]

    port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)
    self.yibu_fan_effect_show_hide(effect, fan_id)
    self.yibu_fan_effect_set_sync(port.ARGB_TEN_MODEL1[fan_id][fan_effect_id])
    if port.MCU_C_FLAG[0] == 0:
        if port.fan_advanced_effect[13] == 0:
            self.yibu_effect_send(fan_id)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.yibu_fan_effect_show_hide (linha 26574)
print(f"同步灯效设置界面显示选项,{index}")
if port.ARGB_effect_check_list[index][1]:
    self.yibu_effect_liusu_group.show()
else:
    self.yibu_effect_liusu_group.hide()
if port.ARGB_effect_check_list[index][2]:
    self.yibu_effect_fangxiang_group.show()
else:
    self.yibu_effect_fangxiang_group.hide()
if port.ARGB_effect_check_list[index][3]:
    self.yibu_effect_liangdu_group.show()
else:
    self.yibu_effect_liangdu_group.hide()
if index == 0:
    print(f"产品ID：{port.ARGB_P_COMPAY}")
    if port.PID_COMPAY == 5 or port.ARGB_P_COMPAY == [0, 0, 0, 5]:
        print(f"产品ID：{port.ARGB_P_COMPAY}")
        if port.ARGB_HEX_SN == 1:
            if port.ARGB_TEN_LEIXING[fan_id] == 1:
                if port.fan_advanced_effect[5] != 2:
                    beilv = port.ARGB_TEN_NUM[fan_id] // 26
                    port.ARGB_effect_check_list[index][4] = beilv
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 7:
                port.ARGB_effect_check_list[index][4] = 4
            elif port.ARGB_TEN_LEIXING[fan_id] == 9:
                if port.fan_advanced_effect[5] != 2:
                    beilv = port.ARGB_TEN_NUM[fan_id] // 18
                    port.ARGB_effect_check_list[index][4] = beilv
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 11 or port.ARGB_TEN_LEIXING[fan_id] == 18:
                if port.fan_advanced_effect[5] == 0:
                    port.ARGB_effect_check_list[index][4] = 3
                elif port.fan_advanced_effect[5] == 5:
                    port.ARGB_effect_check_list[index][4] = 4
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 16:
                if port.fan_advanced_effect[5] != 2:
                    beilv = port.ARGB_TEN_NUM[fan_id] // 34
                    port.ARGB_effect_check_list[index][4] = beilv
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 17:
                port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 21:
                beilv = port.ARGB_TEN_NUM[fan_id] // 11
                port.ARGB_effect_check_list[index][4] = beilv
            elif port.ARGB_TEN_LEIXING[fan_id] == 23:
                if port.fan_advanced_effect[5] != 5:
                    port.ARGB_effect_check_list[index][4] = 1
                else:
                    port.ARGB_effect_check_list[index][4] = 2
            elif port.ARGB_TEN_LEIXING[fan_id] == 24:
                if port.fan_advanced_effect[5] == 0:
                    port.ARGB_effect_check_list[index][4] = 2
                elif port.fan_advanced_effect[5] != 0 and port.fan_advanced_effect[5] != 5:
                    port.ARGB_effect_check_list[index][4] = 1
                else:
                    port.ARGB_effect_check_list[index][4] = 3
            if port.fan_advanced_effect[19][0] == 1 and fan_id == port.fan_advanced_effect[19][1]:
                if port.fan_advanced_effect[5] == 2:
                    port.ARGB_effect_check_list[index][4] = 1
                else:
                    port.ARGB_effect_check_list[index][4] = 4
        else:
            port.ARGB_effect_check_list[index][4] = 1
    elif port.PID_COMPAY == 9 or port.ARGB_P_COMPAY == [0, 0, 0, 9]:
        if port.ARGB_HEX_SN == 1:
            if port.ARGB_TEN_LEIXING[fan_id] == 27:
                if port.fan_advanced_effect[5] == 0:
                    port.ARGB_effect_check_list[index][4] = 1
                elif port.fan_advanced_effect[5] == 5:
                    port.ARGB_effect_check_list[index][4] = 4
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 28:
                if port.fan_advanced_effect[5] == 0:
                    port.ARGB_effect_check_list[index][4] = 2
                elif port.fan_advanced_effect[5] == 5:
                    port.ARGB_effect_check_list[index][4] = 4
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            elif port.ARGB_TEN_LEIXING[fan_id] == 29:
                if port.fan_advanced_effect[5] == 0:
                    port.ARGB_effect_check_list[index][4] = 3
                elif port.fan_advanced_effect[5] == 5:
                    port.ARGB_effect_check_list[index][4] = 4
                else:
                    port.ARGB_effect_check_list[index][4] = 1
            if port.fan_advanced_effect[19][0] == 1 and fan_id == port.fan_advanced_effect[19][1]:
                if port.fan_advanced_effect[5] == 0:
                    port.ARGB_effect_check_list[index][4] = 3
                elif port.fan_advanced_effect[5] == 5:
                    port.ARGB_effect_check_list[index][4] = 4
                else:
                    port.ARGB_effect_check_list[index][4] = 1
        else:
            port.ARGB_effect_check_list[index][4] = 1
    else:
        port.ARGB_effect_check_list[index][4] = 1
if port.ARGB_effect_check_list[index][4]:
    self.yibu_effect_yanse_group.show()
    for i in range(4):
        if i < port.ARGB_effect_check_list[index][4]:
            self.yibu_fan_dengxiao_buttons[i].show()
        else:
            self.yibu_fan_dengxiao_buttons[i].hide()

else:
    self.yibu_effect_yanse_group.hide()
# ==== .MainWindow.yibu_fan_effect_set_sync (linha 26697)
print(f"灯效设置绑定数据: {fan_order_list}")
self.yibu_slider_liusu.blockSignals(True)
for i in range(2):
    self.yibu_fan_fangxiang_buttons[i].blockSignals(True)

self.yibu_slider_liangdu.blockSignals(True)
for i in range(4):
    self.yibu_fan_dengxiao_buttons[i].blockSignals(True)

speed = 60 - fan_order_list[1] * 10
self.yibu_slider_liusu.setValue(speed)
if speed == 0:
    speed = 0.5
self.yibu_liusu_lable.setText(str(int(speed * 2)) + "%")
if fan_order_list[2]:
    self.yibu_fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[0]))))
    self.yibu_fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[1]))))
    self.yibu_fan_fangxiang_buttons[0].setChecked(False)
    self.yibu_fan_fangxiang_buttons[1].setChecked(True)
    self.yibu_fangxiang_lable.setText(self.zheng_daoxu_flag[0])
else:
    self.yibu_fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[0]))))
    self.yibu_fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[1]))))
    self.yibu_fan_fangxiang_buttons[1].setChecked(False)
    self.yibu_fan_fangxiang_buttons[0].setChecked(True)
    self.yibu_fangxiang_lable.setText(self.zheng_daoxu_flag[1])
liangdu = fan_order_list[3] // 51 * 10
self.yibu_liangdu_lable.setText(str(int(fan_order_list[3] / 2.55)) + "%")
if liangdu < 20:
    liangdu = 1
    self.yibu_liangdu_lable.setText(str(1) + "%")
self.yibu_slider_liangdu.setValue(liangdu)
rgb_list_12 = fan_order_list[4:16]
rgb_list_12_0 = rgb_list_12[0:3]
rgb_list_12_1 = rgb_list_12[3:6]
rgb_list_12_2 = rgb_list_12[6:9]
rgb_list_12_3 = rgb_list_12[9:12]
rgb_4 = [rgb_list_12_0, rgb_list_12_1, rgb_list_12_2, rgb_list_12_3]
for i in range(4):
    self.yibu_fan_dengxiao_buttons[i].setStyleSheet(f"background-color: rgb{tuple(rgb_4[i])}; border-radius: 20px;")
    self.yibu_fan_dengxiao_buttons[i].setChecked(False)

self.yibu_button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 20px;")
self.yibu_slider_liusu.blockSignals(False)
for i in range(2):
    self.yibu_fan_fangxiang_buttons[i].blockSignals(False)

self.yibu_slider_liangdu.blockSignals(False)
for i in range(4):
    self.yibu_fan_dengxiao_buttons[i].blockSignals(False)
# ==== .MainWindow.yibu_tiaose_changer (linha 26757)
color_dialog = QColorDialog(self)
color_dialog.setOption(QColorDialog.ColorDialogOption.DontUseNativeDialog)
color = color_dialog.getColor()
if color.isValid():
    self.fan_tiaose_color = test.hex_to_rgb(color.name())
    print("选择颜色:", color.name(), self.fan_tiaose_color)
    self.yibu_button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 16px;")
else:
    print("取消选择颜色")
    return
for i, btn in enumerate(self.yibu_fan_dengxiao_buttons):
    btn.setChecked(False)
# ==== .MainWindow.yibu_yanse_changer (linha 26774)
self.fan_sender_color = self.sender()
if self.fan_sender_color.isChecked():
    index = self.yibu_fan_dengxiao_buttons.index(self.fan_sender_color)
    self.fan_sender_color.setStyleSheet(f"QPushButton {{ background-image: url(); background-repeat: no-repeat; background-position: center; background-color: rgb{tuple(self.fan_tiaose_color)}; border-radius: 20px; }}")
    print(f"Button {index} is checked")
    effect = port.fan_advanced_effect[4]
    fan_id = port.fan_advanced_effect[3]
    fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        port.ARGB_dan_config[fan_effect_id][effect][fan_id][3 + index * 3] = self.fan_tiaose_color[0]
        port.ARGB_dan_config[fan_effect_id][effect][fan_id][3 + index * 3 + 1] = self.fan_tiaose_color[1]
        port.ARGB_dan_config[fan_effect_id][effect][fan_id][3 + index * 3 + 2] = self.fan_tiaose_color[2]
        effect_id = [effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
        port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)
    else:
        fan_effect_id = 5
        port.ARGB_dan_config[5][effect][fan_id][3 + index * 3] = self.fan_tiaose_color[0]
        port.ARGB_dan_config[5][effect][fan_id][3 + index * 3 + 1] = self.fan_tiaose_color[1]
        port.ARGB_dan_config[5][effect][fan_id][3 + index * 3 + 2] = self.fan_tiaose_color[2]
        effect_id = [effect] + port.ARGB_dan_config[5][effect][fan_id]
        port.ARGB_TEN_MODEL1[fan_id][5] = copy.deepcopy(effect_id)
    if port.ARGB_HEX_SN == 0:
        print(f"产品ID：{port.ARGB_P_COMPAY}")
        if effect == 0:
            for i in range(4):
                port.ARGB_dan_config[fan_effect_id][effect][fan_id][3 + i * 3] = self.fan_tiaose_color[0]
                port.ARGB_dan_config[fan_effect_id][effect][fan_id][3 + i * 3 + 1] = self.fan_tiaose_color[1]
                port.ARGB_dan_config[fan_effect_id][effect][fan_id][3 + i * 3 + 2] = self.fan_tiaose_color[2]
                effect_id = [effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
                port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)

    port.write_argb_fan_all()
    port.argb_jiyi_write()
    self.yibu_effect_send(fan_id)
    for button in self.yibu_fan_dengxiao_buttons:
        if button is not self.fan_sender_color:
            button.setChecked(False)
# ==== .MainWindow.yibu_effect_liusu_changer (linha 26817)
liusu = self.yibu_slider_liusu.value()
fan_liusu = (60 - liusu) // 10
if liusu % 10 != 0:
    temp_vaule = 10
else:
    temp_vaule = 0
liusu = liusu // 10 * 10 + temp_vaule
self.yibu_slider_liusu.setValue(liusu)
print(f"风扇流速：{fan_liusu}, {liusu}")
if liusu == 0:
    liusu = 0.5
self.yibu_liusu_lable.setText(str(int(liusu * 2)) + "%")
effect = port.fan_advanced_effect[4]
fan_id = port.fan_advanced_effect[3]
fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
    fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
else:
    fan_effect_id = 5
if port.ARGB_dan_config[fan_effect_id][effect][fan_id][0] != fan_liusu:
    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        port.ARGB_dan_config[fan_effect_id][effect][fan_id][0] = fan_liusu
        effect_id = [effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
        port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)
    else:
        port.ARGB_dan_config[5][effect][fan_id][0] = fan_liusu
        effect_id = [effect] + port.ARGB_dan_config[5][effect][fan_id]
        port.ARGB_TEN_MODEL1[fan_id][5] = copy.deepcopy(effect_id)
    self.yibu_effect_send(fan_id)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.yibu_effect_liangdu_changer (linha 26852)
liangdu = self.yibu_slider_liangdu.value()
fan_liangdu = liangdu // 10 * 51
liangdu = liangdu // 10 * 10
self.yibu_slider_liangdu.setValue(liangdu)
self.yibu_liangdu_lable.setText(str(int(fan_liangdu / 2.55)) + "%")
if fan_liangdu == 0:
    fan_liangdu = 50
    self.yibu_liangdu_lable.setText(str(1) + "%")
effect = port.fan_advanced_effect[4]
fan_id = port.fan_advanced_effect[3]
fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
    fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
else:
    fan_effect_id = 5
if port.ARGB_dan_config[fan_effect_id][effect][fan_id][2] != fan_liangdu:
    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        port.ARGB_dan_config[fan_effect_id][effect][fan_id][2] = fan_liangdu
        effect_id = [effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
        port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)
    else:
        port.ARGB_dan_config[5][effect][fan_id][2] = fan_liangdu
        effect_id = [effect] + port.ARGB_dan_config[5][effect][fan_id]
        port.ARGB_TEN_MODEL1[fan_id][5] = copy.deepcopy(effect_id)
    self.yibu_effect_send(fan_id)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.yibu_effect_fangxiang_changer (linha 26882)
fan_fangxiang_sender = self.sender()
if fan_fangxiang_sender.isChecked():
    index = self.yibu_fan_fangxiang_buttons.index(fan_fangxiang_sender)
    print(f"Button {index} is checked")
    for button in self.yibu_fan_fangxiang_buttons:
        if button is not fan_fangxiang_sender:
            button.setChecked(False)
        else:
            self.yibu_fan_fangxiang_buttons[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[index]))))

    for i, btn in enumerate(self.yibu_fan_fangxiang_buttons):
        if i != index:
            self.yibu_fan_fangxiang_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[i]))))
        effect = port.fan_advanced_effect[4]
        fan_id = port.fan_advanced_effect[3]
        fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]

    if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
        fan_effect_id = port.gaoji_ten_dengxiao_leixing[fan_id][2]
    else:
        fan_effect_id = 5
    if port.ARGB_dan_config[fan_effect_id][effect][fan_id][1] != index:
        if port.ARGB_TEN_LEIXING[fan_id] in port.jimu_lianpai:
            port.ARGB_dan_config[fan_effect_id][effect][fan_id][1] = index
            effect_id = [effect] + port.ARGB_dan_config[fan_effect_id][effect][fan_id]
            port.ARGB_TEN_MODEL1[fan_id][fan_effect_id] = copy.deepcopy(effect_id)
        else:
            port.ARGB_dan_config[5][effect][fan_id][1] = index
            effect_id = [effect] + port.ARGB_dan_config[5][effect][fan_id]
            port.ARGB_TEN_MODEL1[fan_id][5] = copy.deepcopy(effect_id)
        self.yibu_effect_send(fan_id)
        port.write_argb_fan_all()
        port.argb_jiyi_write()
    if index:
        self.yibu_fangxiang_lable.setText(self.zheng_daoxu_flag[0])
    else:
        self.yibu_fangxiang_lable.setText(self.zheng_daoxu_flag[1])
# ==== .MainWindow.hebing_effect_changer_Clicked (linha 26922)
sender = self.sender()
index = self.hebing_effect_buttons.index(sender)
self.hebing_effect_buttons[index].setChecked(True)
self.hebing_effect_buttons[index].setIcon(QIcon(port.file_path_img(self.hebing_effect_imgs[index])))
self.hebing_effect_lables[index].setStyleSheet("color: rgb(242, 244, 241); font-size: 14px; ")
port.fan_advanced_effect[2] = index
if sender.isChecked():
    print(f"Button {port.dengxiao_model2_zh[index]} is checked")
    for i, button in enumerate(self.hebing_effect_buttons):
        if button is not sender:
            button.setChecked(False)
            self.hebing_effect_buttons[i].setIcon(QIcon(port.file_path_img(self.hebing_effect_imgs_xuanzhong[i])))
            self.hebing_effect_lables[i].setStyleSheet("color: rgb(100, 100, 100); font-size: 14px; ")
        agreement.fan_order_list[0] = index

    for i in range(15):
        agreement.fan_order_list[1 + i] = port.ARGB_qun_async_config[index][i]

    if port.MCU_C_FLAG[0] == 0:
        if port.fan_advanced_effect[13] == 0:
            agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
self.hebing_fan_effect_set_sync(agreement.fan_order_list)
hebing_index = self.hebing_effect_list[index]
self.hebing_fan_effect_show_hide(hebing_index)
port.write_argb_fan_all()
port.argb_jiyi_write()
# ==== .MainWindow.hebing_fan_effect_show_hide (linha 26949)
print(f"合并灯效设置界面显示选项, {index}")
self.hebing_effect_liusu_group.setVisible(port.ARGB_effect_check_list[index][1])
print(f"方向是否隐藏：{port.ARGB_effect_check_list[index][2]}")
self.hebing_effect_fangxiang_group.setVisible(port.ARGB_effect_check_list[index][2])
self.hebing_effect_liangdu_group.setVisible(port.ARGB_effect_check_list[index][3])
has_color = port.ARGB_effect_check_list[index][4]
self.hebing_effect_yanse_group.setVisible(has_color)
if has_color:
    color_count = port.ARGB_effect_check_list[index][4]
    for i in range(4):
        self.hebing_fan_dengxiao_buttons[i].setVisible(i < color_count)
# ==== .MainWindow.hebing_fan_effect_set_sync (linha 26972)
print(f"灯效设置绑定数据: {fan_order_list}")
self.hebing_slider_liusu.blockSignals(True)
for i in range(2):
    self.hebing_fan_fangxiang_buttons[i].blockSignals(True)

self.hebing_slider_liangdu.blockSignals(True)
for i in range(4):
    self.hebing_fan_dengxiao_buttons[i].blockSignals(True)

speed = 60 - fan_order_list[1] * 10
self.hebing_slider_liusu.setValue(speed)
if speed == 0:
    speed = 0.5
self.hebing_liusu_lable.setText(str(int(speed * 2)) + "%")
if fan_order_list[2]:
    self.hebing_fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[0]))))
    self.hebing_fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[1]))))
    self.hebing_fan_fangxiang_buttons[0].setChecked(False)
    self.hebing_fan_fangxiang_buttons[1].setChecked(True)
    self.hebing_fangxiang_lable.setText(self.zheng_daoxu_flag[0])
else:
    self.hebing_fan_fangxiang_buttons[0].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[0]))))
    self.hebing_fan_fangxiang_buttons[1].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[1]))))
    self.hebing_fan_fangxiang_buttons[1].setChecked(False)
    self.hebing_fan_fangxiang_buttons[0].setChecked(True)
    self.hebing_fangxiang_lable.setText(self.zheng_daoxu_flag[1])
liangdu = fan_order_list[3] // 51 * 10
self.hebing_liangdu_lable.setText(str(int(fan_order_list[3] / 2.55)) + "%")
if liangdu < 20:
    liangdu = 1
    self.hebing_liangdu_lable.setText(str(1) + "%")
self.hebing_slider_liangdu.setValue(liangdu)
rgb_list_12 = fan_order_list[4:16]
rgb_list_12_0 = rgb_list_12[0:3]
rgb_list_12_1 = rgb_list_12[3:6]
rgb_list_12_2 = rgb_list_12[6:9]
rgb_list_12_3 = rgb_list_12[9:12]
rgb_4 = [rgb_list_12_0, rgb_list_12_1, rgb_list_12_2, rgb_list_12_3]
for i in range(4):
    self.hebing_fan_dengxiao_buttons[i].setStyleSheet(f"background-color: rgb{tuple(rgb_4[i])}; border-radius: 20px;")
    self.hebing_fan_dengxiao_buttons[i].setChecked(False)

self.hebing_button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 16px;")
self.hebing_slider_liusu.blockSignals(False)
for i in range(2):
    self.hebing_fan_fangxiang_buttons[i].blockSignals(False)

self.hebing_slider_liangdu.blockSignals(False)
for i in range(4):
    self.hebing_fan_dengxiao_buttons[i].blockSignals(False)
# ==== .MainWindow.hebing_tiaose_changer (linha 27032)
color_dialog = QColorDialog(self)
color_dialog.setOption(QColorDialog.ColorDialogOption.DontUseNativeDialog)
color = color_dialog.getColor()
if color.isValid():
    self.fan_tiaose_color = test.hex_to_rgb(color.name())
    print("选择颜色:", color.name(), self.fan_tiaose_color)
    self.hebing_button_fan_tiaose.setStyleSheet(f"background-color: rgb{tuple(self.fan_tiaose_color)}; color: white; border-radius: 15px; font-size: 16px;")
else:
    print("取消选择颜色")
    return
for i, btn in enumerate(self.hebing_fan_dengxiao_buttons):
    btn.setChecked(False)
# ==== .MainWindow.hebing_yanse_changer (linha 27049)
self.fan_sender_color = self.sender()
if self.fan_sender_color.isChecked():
    index = self.hebing_fan_dengxiao_buttons.index(self.fan_sender_color)
    self.fan_sender_color.setStyleSheet(f"QPushButton {{ background-image: url(); background-repeat: no-repeat; background-position: center; background-color: rgb{tuple(self.fan_tiaose_color)}; border-radius: 20px; }}")
    print(f"Button {index} is checked")
    colors_four_list = [[0, 0, 0] for _ in range(4)]
    effect = port.fan_advanced_effect[2]
    agreement.fan_order_list[0] = effect
    port.ARGB_qun_async_config[effect][3 + index * 3] = self.fan_tiaose_color[0]
    port.ARGB_qun_async_config[effect][3 + index * 3 + 1] = self.fan_tiaose_color[1]
    port.ARGB_qun_async_config[effect][3 + index * 3 + 2] = self.fan_tiaose_color[2]
    for index, vaule in enumerate(port.ARGB_qun_async_config[effect]):
        agreement.fan_order_list[index + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
    for i in range(4):
        colors_four_list[i][0] = port.ARGB_qun_async_config[effect][3 + i * 3]
        colors_four_list[i][1] = port.ARGB_qun_async_config[effect][3 + i * 3 + 1]
        colors_four_list[i][2] = port.ARGB_qun_async_config[effect][3 + i * 3 + 2]

    port.write_argb_fan_all()
    port.argb_jiyi_write()
    for button in self.hebing_fan_dengxiao_buttons:
        if button is not self.fan_sender_color:
            button.setChecked(False)

    for i, btn in enumerate(self.hebing_fan_dengxiao_buttons):
        try:
            if i != index:
                btn.setStyleSheet(f"background-color: rgb{tuple(colors_four_list[i])}; border-radius: 20px;")
        except:
            pass
# ==== .MainWindow.hebing_effect_liusu_changer (linha 27084)
liusu = self.hebing_slider_liusu.value()
fan_liusu = (60 - liusu) // 10
if liusu % 10 != 0:
    temp_vaule = 10
else:
    temp_vaule = 0
liusu = liusu // 10 * 10 + temp_vaule
self.hebing_slider_liusu.setValue(liusu)
print(f"风扇流速：{fan_liusu}, {liusu}")
if liusu == 0:
    liusu = 0.5
self.hebing_liusu_lable.setText(str(int(liusu * 2)) + "%")
effect = port.fan_advanced_effect[2]
agreement.fan_order_list[0] = effect
if port.ARGB_qun_async_config[effect][0] != fan_liusu:
    port.ARGB_qun_async_config[effect][0] = fan_liusu
    for index, vaule in enumerate(port.ARGB_qun_async_config[effect]):
        agreement.fan_order_list[index + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.hebing_effect_liangdu_changer (linha 27109)
liangdu = self.hebing_slider_liangdu.value()
fan_liangdu = liangdu // 10 * 51
liangdu = liangdu // 10 * 10
self.hebing_slider_liangdu.setValue(liangdu)
self.hebing_liangdu_lable.setText(str(int(fan_liangdu / 2.55)) + "%")
if fan_liangdu == 0:
    fan_liangdu = 50
    self.hebing_liangdu_lable.setText(str(1) + "%")
effect = port.fan_advanced_effect[2]
agreement.fan_order_list[0] = effect
if port.ARGB_qun_async_config[effect][2] != fan_liangdu:
    port.ARGB_qun_async_config[effect][2] = fan_liangdu
    for index, vaule in enumerate(port.ARGB_qun_async_config[effect]):
        agreement.fan_order_list[index + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
# ==== .MainWindow.hebing_effect_fangxiang_changer (linha 27129)
fan_fangxiang_sender = self.sender()
if fan_fangxiang_sender.isChecked():
    index = self.hebing_fan_fangxiang_buttons.index(fan_fangxiang_sender)
    print(f"Button {index} is checked")
    for button in self.hebing_fan_fangxiang_buttons:
        if button is not fan_fangxiang_sender:
            button.setChecked(False)
        else:
            self.hebing_fan_fangxiang_buttons[index].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img_xuanzhong[index]))))

    for i, btn in enumerate(self.hebing_fan_fangxiang_buttons):
        if i != index:
            self.hebing_fan_fangxiang_buttons[i].setIcon(QIcon(port.file_path_img(port.file_path_img(self.fan_fangxiang_img[i]))))
        effect = port.fan_advanced_effect[2]
        agreement.fan_order_list[0] = effect
        port.ARGB_qun_async_config[effect][1] = index

    for i, vaule in enumerate(port.ARGB_qun_async_config[effect]):
        agreement.fan_order_list[i + 1] = vaule

    agreement.ARGB_ten_effect(port.ARGB_ser, 254, 254)
    port.write_argb_fan_all()
    port.argb_jiyi_write()
    if index:
        self.hebing_fangxiang_lable.setText(self.zheng_daoxu_flag[0])
    else:
        self.hebing_fangxiang_lable.setText(self.zheng_daoxu_flag[1])