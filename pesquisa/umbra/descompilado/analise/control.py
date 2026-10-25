
# ==== .ControlPanel.__init__ (linha 12598)
super().__init__(parent)
self.effect_manager = effect_manager
self.canvas_scene = canvas_scene
self.init_ui()
# ==== .ControlPanel.init_ui (linha 12606)
main_layout = QVBoxLayout()
self.effect_selector_group = QGroupBox("灯效选择")
self.effect_selector_group.setStyleSheet("\n            QGroupBox {\n                color: white;\n                border: 1px solid gray;\n                border-radius: 3px;\n                margin-top: 0.5em;\n                font-size: 16px;\n            }\n            QGroupBox::title {\n                color: white;\n                subcontrol-origin: margin;\n                left: 10px;\n                padding: 0 3px;\n                font-size: 16px;\n            }\n        ")
selector_layout = QVBoxLayout()
self.effect_combo = QComboBox()
self.effect_combo.setStyleSheet(f"\n            QComboBox {{\n                background-color: {port.xianshi_jianbian};\n                color: {port.back_yanse};\n                border: 1px solid #555;\n                border-radius: 5px;\n                padding: 5px;\n                font-size: 16px;\n            }}\n            QComboBox::drop-down {{\n                subcontrol-origin: padding;\n                subcontrol-position: top right;\n                width: 16px;\n                border-left-width: 1px;\n                border-left-color: rgb(51, 51, 50);\n                border-left-style: solid;\n                border-top-right-radius: 5px;\n                border-bottom-right-radius: 5px;\n                font-size: 16px;\n            }}\n            /* 下拉列表整体样式 */\n            QComboBox QAbstractItemView {{\n                background-color: {port.xianshi_jianbian};\n                color: {port.back_yanse};\n                border-radius: 5px;\n                outline: none;  /* 移除选中虚线框 */\n                font-size: 16px;\n\n            }}\n            /* 悬停项样式 */\n            QComboBox QAbstractItemView::item:hover {{\n                background-color: rgb(0, 0, 0) !important;\n                color: {port.back_yanse} !important;\n                border: 1px solid {port.back_yanse} !important;\n                border-radius: 3px;\n                font-size: 16px;\n            }}")
self.populate_effect_combo()
self.effect_combo.currentIndexChanged.connect(self.on_effect_changed)
selector_layout.addWidget(self.effect_combo)
self.effect_selector_group.setLayout(selector_layout)
main_layout.addWidget(self.effect_selector_group)
self.effect_settings_group = QGroupBox("灯效设置")
self.effect_settings_group.setStyleSheet("\n            QGroupBox {\n                color: white;\n                border: 1px solid gray;\n                border-radius: 3px;\n                margin-top: 0.5em;\n                font-size: 16px;\n            }\n            QGroupBox::title {\n                color: white;\n                subcontrol-origin: margin;\n                left: 10px;\n                padding: 0 3px;\n                font-size: 16px;\n            }\n        ")
effect_settings_layout = QVBoxLayout()
liangdu_layout = QVBoxLayout()
liangdu_layout_text = QHBoxLayout()
self.liangdu_label_text = QLabel("亮度: ")
self.liangdu_label_text.setStyleSheet("color: white; font-size: 16px;")
self.liangdu_slider = QSlider(Qt.Horizontal)
self.liangdu_slider.setStyleSheet(f"\n        /* 滑块样式 */\n        QSlider::handle:horizontal {{\n            background: yellow;\n            width: 16px;          \n            height: 16px;         \n            margin: -5px 0;       \n            border-radius: 8px;    \n        }}\n        \n        /* 轨道样式 */\n        QSlider::groove:horizontal {{\n            height: 8px;\n            background: rgb(128, 128, 128);    \n            border-radius: 4px;\n        }}\n        \n        /* 滑块悬停状态 */\n        QSlider::handle:horizontal:hover {{\n            background: {port.xianshi_jianbian};  \n        }}\n        \n        /* 滑块按下状态 */\n        QSlider::handle:horizontal:pressed {{\n            background: {port.xianshi_jianbian};  \n        }}\n    ")
self.liangdu_slider.setRange(1, 10)
liangdu = self.effect_manager.effect_brightness
self.liangdu_slider.setValue(liangdu)
self.liangdu_slider.setTracking(True)

def liangdu_mousePressEvent(event):
    if event.button() == Qt.LeftButton:
        pos = event.pos().x()
        width = self.liangdu_slider.width()
        value = round(1 + pos / width * 9)
        self.liangdu_slider.setValue(value)
        event.accept()
    else:
        QSlider.mousePressEvent(self.liangdu_slider, event)


self.liangdu_slider.mousePressEvent = lambda event: liangdu_mousePressEvent(event)
self.liangdu_slider.valueChanged.connect(self.update_effect_liangdu)
self.liangdu_label = QLabel((f"{liangdu}"))
self.liangdu_label.setStyleSheet("color: white; font-size: 16px;")
liangdu_layout_text.addWidget(self.liangdu_label_text)
liangdu_layout_text.addWidget(self.liangdu_label)
liangdu_layout_text.addStretch(1)
liangdu_layout.addLayout(liangdu_layout_text)
liangdu_layout.addWidget(self.liangdu_slider)
speed_layout = QVBoxLayout()
speed_layout_text = QHBoxLayout()
self.speed_label_text = QLabel("速度: ")
self.speed_label_text.setStyleSheet("color: white; font-size: 16px;")
self.speed_slider = QSlider(Qt.Horizontal)
self.speed_slider.setStyleSheet(f"\n        /* 滑块样式 */\n        QSlider::handle:horizontal {{\n            background: yellow;\n            width: 16px;          \n            height: 16px;         \n            margin: -5px 0;       \n            border-radius: 8px;    \n        }}\n        \n        /* 轨道样式 */\n        QSlider::groove:horizontal {{\n            height: 8px;\n            background: rgb(128, 128, 128);    \n            border-radius: 4px;\n        }}\n        \n        /* 滑块悬停状态 */\n        QSlider::handle:horizontal:hover {{\n            background: {port.xianshi_jianbian};  \n        }}\n        \n        /* 滑块按下状态 */\n        QSlider::handle:horizontal:pressed {{\n            background: {port.xianshi_jianbian};  \n        }}\n    ")
self.speed_slider.setRange(1, 10)
speed = self.effect_manager.effects[self.effect_manager.current_effect]["speed"]
self.speed_slider.setValue(speed)
self.speed_slider.setTracking(True)

def mousePressEvent(event):
    if event.button() == Qt.LeftButton:
        pos = event.pos().x()
        width = self.speed_slider.width()
        value = round(1 + pos / width * 9)
        self.speed_slider.setValue(value)
        event.accept()
    else:
        QSlider.mousePressEvent(self.speed_slider, event)


self.speed_slider.mousePressEvent = lambda event: mousePressEvent(event)
self.speed_slider.valueChanged.connect(self.update_effect_params)
self.speed_label = QLabel((f"{speed}"))
self.speed_label.setStyleSheet("color: white;font-size: 16px;")
speed_layout_text.addWidget(self.speed_label_text)
speed_layout_text.addWidget(self.speed_label)
speed_layout_text.addStretch(1)
speed_layout.addLayout(speed_layout_text)
speed_layout.addWidget(self.speed_slider)
self.color_btn = QPushButton()
self.color_btn.clicked.connect(self.set_effect_color)
self.color_btn.setStyleSheet("color: white;font-size: 16px;")
self.width_slider = QSlider(Qt.Horizontal)
self.width_slider.setStyleSheet(f"\n        /* 滑块样式 */\n        QSlider::handle:horizontal {{\n            background: yellow;\n            width: 16px;          \n            height: 16px;         \n            margin: -5px 0;       \n            border-radius: 8px;    \n        }}\n        \n        /* 轨道样式 */\n        QSlider::groove:horizontal {{\n            height: 8px;\n            background: rgb(128, 128, 128);    \n            border-radius: 4px;\n        }}\n        \n        /* 滑块悬停状态 */\n        QSlider::handle:horizontal:hover {{\n            background: {port.xianshi_jianbian};  \n        }}\n        \n        /* 滑块按下状态 */\n        QSlider::handle:horizontal:pressed {{\n            background: {port.xianshi_jianbian};  \n        }}\n    ")
self.width_slider.setRange(30, 300)
self.width_slider.valueChanged.connect(self.set_effect_width)
kuandu_layout = QHBoxLayout()
self.width_label = QLabel("宽度: ")
self.width_label.setStyleSheet("color: white; font-size: 16px;")
self.width_label_txt = QLabel("100")
self.width_label_txt.setStyleSheet("color: white; font-size: 16px;")
kuandu_layout.addWidget(self.width_label)
kuandu_layout.addWidget(self.width_label_txt)
kuandu_layout.addStretch(0)
self.height_slider = QSlider(Qt.Horizontal)
self.height_slider.setStyleSheet(f"\n        /* 滑块样式 */\n        QSlider::handle:horizontal {{\n            background: yellow;\n            width: 16px;          \n            height: 16px;         \n            margin: -5px 0;       \n            border-radius: 8px;    \n        }}\n        \n        /* 轨道样式 */\n        QSlider::groove:horizontal {{\n            height: 8px;\n            background: rgb(128, 128, 128);    \n            border-radius: 4px;\n        }}\n        \n        /* 滑块悬停状态 */\n        QSlider::handle:horizontal:hover {{\n            background: {port.xianshi_jianbian};  \n        }}\n        \n        /* 滑块按下状态 */\n        QSlider::handle:horizontal:pressed {{\n            background: {port.xianshi_jianbian};  \n        }}\n    ")
self.height_slider.setRange(30, 300)
self.height_slider.valueChanged.connect(self.set_effect_height)
mubu_gaodu_layout = QHBoxLayout()
self.height_label = QLabel("高度: ")
self.height_label.setStyleSheet("color: white; font-size: 16px;")
self.height_label_txt = QLabel("100")
self.height_label_txt.setStyleSheet("color: white; font-size: 16px;")
mubu_gaodu_layout.addWidget(self.height_label)
mubu_gaodu_layout.addWidget(self.height_label_txt)
mubu_gaodu_layout.addStretch(0)
self.color_btns = []
color_btns_layout = QHBoxLayout()
for i in range(4):
    btn = QPushButton()
    btn.clicked.connect(self.create_color_handler(i))
    btn.setStyleSheet("color: white; font-size: 16px;")
    self.color_btns.append(btn)
    color_btns_layout.addWidget(btn)

self.color_btn.hide()
self.width_slider.hide()
self.width_label.hide()
self.width_label_txt.hide()
self.height_slider.hide()
self.height_label.hide()
self.height_label_txt.hide()
for btn in self.color_btns:
    btn.hide()

tongping_layout = QVBoxLayout()
self.pingjun_radio = QRadioButton("平均色")
self.pingjun_radio.setStyleSheet("color: white; font-size: 16px;")
self.zhengping_radio = QRadioButton("整屏")
self.zhengping_radio.setStyleSheet("color: white; font-size: 16px;")
self.pingjun_radio.setChecked(True)
self.pingjun_radio.setProperty("id", "pingjun")
self.zhengping_radio.setProperty("id", "zhengping")
self.tongping_button_group = QButtonGroup()
self.tongping_button_group.addButton(self.pingjun_radio, Qt.Vertical)
self.tongping_button_group.addButton(self.zhengping_radio, Qt.Horizontal)
self.tongping_button_group.buttonClicked.connect(self.changer_tongping_index)
tongping_layout.addWidget(self.pingjun_radio)
tongping_layout.addWidget(self.zhengping_radio)
effect_settings_layout.addLayout(liangdu_layout)
effect_settings_layout.addLayout(speed_layout)
effect_settings_layout.addWidget(self.color_btn)
effect_settings_layout.addLayout(kuandu_layout)
effect_settings_layout.addWidget(self.width_slider)
effect_settings_layout.addLayout(mubu_gaodu_layout)
effect_settings_layout.addWidget(self.height_slider)
effect_settings_layout.addLayout(color_btns_layout)
effect_settings_layout.addLayout(tongping_layout)
self.effect_settings_group.setLayout(effect_settings_layout)
main_layout.addWidget(self.effect_settings_group)
self.orientation_group = QGroupBox("设备方向")
self.orientation_group.setStyleSheet("\n            QGroupBox {\n                color: white;\n                border: 1px solid gray;\n                border-radius: 3px;\n                margin-top: 0.5em;\n                font-size: 16px;\n            }\n            QGroupBox::title {\n                color: white;\n                subcontrol-origin: margin;\n                left: 10px;\n                padding: 0 3px;\n                font-size: 16px;\n            }\n        ")
orientation_layout = QVBoxLayout()
self.vertical_radio = QRadioButton("垂直")
self.vertical_radio.setStyleSheet("color: white;font-size: 16px;")
self.horizontal_radio = QRadioButton("水平")
self.horizontal_radio.setStyleSheet("color: white;font-size: 16px;")
self.vertical_radio.setChecked(True)
self.orientation_button_group = QButtonGroup()
self.orientation_button_group.addButton(self.vertical_radio, Qt.Vertical)
self.orientation_button_group.addButton(self.horizontal_radio, Qt.Horizontal)
self.orientation_button_group.buttonClicked.connect(self.change_icon_orientation)
orientation_layout.addWidget(self.vertical_radio)
orientation_layout.addWidget(self.horizontal_radio)
self.orientation_group.setLayout(orientation_layout)
main_layout.addWidget(self.orientation_group)
self.orientation_group.hide()
self.power_group = QGroupBox("设备电源方向")
self.power_group.setStyleSheet("\n            QGroupBox {\n                color: white;\n                border: 1px solid gray;\n                border-radius: 3px;\n                margin-top: 0.5em;\n                font-size: 16px;\n            }\n            QGroupBox::title {\n                color: white;\n                subcontrol-origin: margin;\n                left: 10px;\n                padding: 0 3px;\n                font-size: 16px;\n            }\n        ")
power_layout = QVBoxLayout()
self.forward_radio = QRadioButton("正序")
self.forward_radio.setStyleSheet("color: white;font-size: 16px;")
self.reverse_radio = QRadioButton("倒序")
self.reverse_radio.setStyleSheet("color: white;font-size: 16px;")
self.forward_radio.setChecked(True)
self.power_button_group = QButtonGroup()
self.power_button_group.addButton(self.forward_radio, 1)
self.power_button_group.addButton(self.reverse_radio, -1)
self.power_button_group.buttonClicked.connect(self.change_power_direction)
power_layout.addWidget(self.forward_radio)
power_layout.addWidget(self.reverse_radio)
self.power_group.setLayout(power_layout)
main_layout.addWidget(self.power_group)
self.power_group.hide()
main_layout.addStretch()
self.setLayout(main_layout)
self.setFixedWidth(200)
# ==== .ControlPanel.init_ui.liangdu_mousePressEvent (linha 12771)
if event.button() == Qt.LeftButton:
    pos = event.pos().x()
    width = self.liangdu_slider.width()
    value = round(1 + pos / width * 9)
    self.liangdu_slider.setValue(value)
    event.accept()
else:
    QSlider.mousePressEvent(self.liangdu_slider, event)
# ==== .ControlPanel.init_ui.mousePressEvent (linha 12837)
if event.button() == Qt.LeftButton:
    pos = event.pos().x()
    width = self.speed_slider.width()
    value = round(1 + pos / width * 9)
    self.speed_slider.setValue(value)
    event.accept()
else:
    QSlider.mousePressEvent(self.speed_slider, event)
# ==== .ControlPanel.populate_effect_combo (linha 13084)
self.effect_combo.clear()
self.buttons_name_en = [
 "rainbow", "wave", "block", "four_directions", "corner_fill",
 "random_circle", "fourth_dimension", "bubble", "dna_helix", "radar",
 "impact", "gradient_push", "music_visualizer", "diagonal_visualizer",
 "music_center_side", "sound_bar_visualizer", "screen_effects", "snake",
 "clock", "fireworks", "matrix_flow", "matrix_rain", "fluid_fill",
 "box_pushing", "black_hole", "custom_wave", "fire", "aurora", "riptide_wave",
 "gradient", "interleave", "spiral_rainbow", "meteor", "sky_cloud", "camo_fade",
 "lasers_lasers", "storm", "explode", "rainbow_tunnel", "saiyan", "spiral", "poison",
 "windmill", "maple_leaf", "lightsaber_battle", "trace_growth", "police_light",
 "sphere_3d", "dream_dream", "neon_wave", "fireflies", "hellfire", "dragon_flame",
 "hydrogen", "hyperspace", "jellyfish", "koi_pond", "lasers", "lava_lamp", "mario",
 "multiverse_simple", "cosmic_nebula", "neon_grid", "neurolink", "new_years",
 "paint_splatters", "party_parrot", "pixel_wave", "planetary_orbit", "plasma_flow",
 "poison_max", "rainbow_tap", "resident_evil", "rick_morty"]
self.buttons_name_zh = [
 "彩虹流动", "波浪", "积木", "四向平铺", "四角填充", "随机圆环",
 "四次元", "气泡", "DNA 螺旋", "雷达扫描", "撞击效果", "渐变推动",
 "音乐1", "音乐2", "音乐3", "音乐4", "同屏", "贪吃蛇", "时钟", "烟花",
 "矩阵流动", "黑客帝国", "流体填充", "推箱子堆叠", "黑洞", "定制波浪",
 "火焰", "极光", "激流", "线性渐变", "交错", "螺旋彩虹", "流星", "蓝天白云",
 "迷彩褪色", "激光", "暴风雨", "爆炸", "彩虹隧道", "超级赛亚人", "定制螺旋", "毒药",
 "风车", "枫叶", "光剑决斗", "轨迹生长", "警灯", "3D球面光环", "迷幻梦境", "霓虹日落波浪",
 "萤火虫", "地狱之火", "龙之焰", "氢原子轨道", "超空间星场", "飘逸水母", "锦鲤池塘",
 "激光舞台", "熔岩灯", "超级马里奥", "多元宇宙", "星云", "霓虹网格", "神经链接",
 "新年烟花", "颜料泼溅", "派对鹦鹉", "像素波浪", "行星轨道", "等离子流动", "剧毒弥漫",
 "彩虹波纹", "生化危机", "瑞克和莫蒂"]
self.effect_combo.addItems(self.buttons_name_zh)
current_effect_en = self.effect_manager.current_effect
if current_effect_en not in self.buttons_name_en:
    current_index = 0
else:
    current_index = self.buttons_name_en.index(current_effect_en)
self.effect_combo.setCurrentIndex(current_index)
# ==== .ControlPanel.on_effect_changed (linha 13146)
if index < 0:
    return
print(self.buttons_name_en)
selected_effect_en = self.buttons_name_en[index]
print(selected_effect_en)
self.effect_manager.current_effect = selected_effect_en
self.effect_manager.save_config()
self.update_control_visibility()
# ==== .ControlPanel.create_color_handler (linha 13164)


def handler():
    self.set_multi_color(index)


return handler
# ==== .ControlPanel.create_color_handler.handler (linha 13166)
self.set_multi_color(index)
# ==== .ControlPanel.change_icon_orientation (linha 13170)
if self.canvas_scene.selected_icon:
    orientation = self.orientation_button_group.id(button)
    self.canvas_scene.selected_icon.set_orientation(orientation)
# ==== .ControlPanel.change_power_direction (linha 13175)
if self.canvas_scene.selected_icon:
    direction = self.power_button_group.id(button)
    self.canvas_scene.selected_icon.set_power_direction(direction)
# ==== .ControlPanel.changer_tongping_index (linha 13180)
self.effect_manager.effects[self.effect_manager.current_effect]["quse"]
if radio_button.property("id") == "pingjun":
    print("平均色")
    self.effect_manager.effects[self.effect_manager.current_effect]["quse"] = 0
elif radio_button.property("id") == "zhengping":
    print("整屏")
    self.effect_manager.effects[self.effect_manager.current_effect]["quse"] = 1
self.effect_manager.save_config()
# ==== .ControlPanel.update_control_visibility (linha 13191)
# descompilação falhou (demorou demais); desmontagem:
# # pydisasm version 6.1.7
# # Python bytecode 3.8.0 ()
# # Disassembled from Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# # Timestamp in code: 0 (1969-12-31 21:00:00)
# # Method Name:       update_control_visibility
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  8
# # Stack size:        6
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        13191
# # Constants:
# #    0: None
# #    1: 'speed'
# #    2: 'type'
# #    3: 'wave'
# #    4: 'width'
# #    5: 'color'
# #    6: 'block'
# #    7: 'height'
# #    8: 'four_directions'
# #    9: 'colors'
# #   10: 'corner_fill'
# #   11: 'bubble'
# #   12: 2
# #   13: 'dna_helix'
# #   14: 'radar'
# #   15: 'diagonal_visualizer'
# #   16: 'music_center_side'
# #   17: 'screen_effects'
# #   18: 'quse'
# #   19: 0
# #   20: True
# #   21: 'clock'
# #   22: 'fireworks'
# #   23: 'matrix_flow'
# #   24: 'fluid_fill'
# #   25: 'box_pushing'
# #   26: 'lightsaber_duel'
# #   27: 'black_hole'
# #   28: 4
# #   29: 'custom_wave'
# #   30: 'n_colors'
# #   31: 'fire'
# #   32: 'riptide_wave'
# #   33: 3
# #   34: 'gradient'
# #   35: 'interleave'
# #   36: 'meteor'
# #   37: 'sky_cloud'
# #   38: 'camo_fade'
# #   39: 'lasers_lasers'
# #   40: 'storm'
# #   41: 'explode'
# #   42: 'saiyan'
# #   43: 'spiral'
# #   44: 'poison'
# #   45: 'windmill'
# #   46: 'maple_leaf'
# #   47: 'lightsaber_battle'
# #   48: 'police_light'
# #   49: 'sphere_3d'
# #   50: 'dream_dream'
# #   51: 'neon_wave'
# #   52: 'fireflies'
# #   53: 'hellfire'
# #   54: 'dragon_flame'
# #   55: 'hydrogen'
# #   56: 'hyperspace'
# #   57: 'jellyfish'
# #   58: 'koi_pond'
# #   59: 'lasers'
# #   60: 'lava_lamp'
# #   61: 'mario'
# #   62: 'multiverse_simple'
# #   63: 'cosmic_nebula'
# #   64: 'neon_grid'
# #   65: 'neurolink'
# #   66: 'new_years'
# #   67: 'paint_splatters'
# #   68: 'party_parrot'
# #   69: 'pixel_wave'
# #   70: 'planetary_orbit'
# #   71: 'plasma_flow'
# #   72: 'poison_max'
# #   73: 'rainbow_tap'
# #   74: 'resident_evil'
# #   75: 'rick_morty'
# #   76: 1
# #   77: 255
# #   78: '图标取消选择'
# # Names:
# #    0: effect_manager
# #    1: effects
# #    2: current_effect
# #    3: speed_slider
# #    4: setValue
# #    5: speed_label
# #    6: setText
# #    7: speed_label_text
# #    8: show
# #    9: color_btn
# #   10: hide
# #   11: width_slider
# #   12: width_label
# #   13: width_label_txt
# #   14: height_slider
# #   15: height_label
# #   16: height_label_txt
# #   17: color_btns
# #   18: pingjun_radio
# #   19: zhengping_radio
# #   20: str
# #   21: update_color_btn
# #   22: enumerate
# #   23: set_button_color
# #   24: setChecked
# #   25: len
# #   26: canvas_scene
# #   27: selected_icon
# #   28: orientation_group
# #   29: power_group
# #   30: orientation
# #   31: Qt
# #   32: Vertical
# #   33: vertical_radio
# #   34: horizontal_radio
# #   35: power_direction
# #   36: forward_radio
# #   37: reverse_radio
# #   38: device_xuanzhong_flag
# #   39: print
# # Varnames:
# #	self, speed, effect_type, effect, btn, i, color, n_colors
# # Positional arguments:
# #	self
# # Local variables:
# #    1: speed
# #    2: effect_type
# #    3: effect
# #    4: btn
# #    5: i
# #    6: color
# #    7: n_colors
# 
# # Method Name:       update_control_visibility
# # Filename:          Sync_light.py
# # Argument count:    1
# # Position-only argument count: 0
# # Keyword-only arguments: 0
# # Number of locals:  8
# # Stack size:        6
# # Flags:             0x00000043 (NOFREE | NEWLOCALS | OPTIMIZED)
# # First Line:        13191
# # Constants:
# #    0: None
# #    1: 'speed'
# #    2: 'type'
# #    3: 'wave'
# #    4: 'width'
# #    5: 'color'
# #    6: 'block'
# #    7: 'height'
# #    8: 'four_directions'
# #    9: 'colors'
# #   10: 'corner_fill'
# #   11: 'bubble'
# #   12: 2
# #   13: 'dna_helix'
# #   14: 'radar'
# #   15: 'diagonal_visualizer'
# #   16: 'music_center_side'
# #   17: 'screen_effects'
# #   18: 'quse'
# #   19: 0
# #   20: True
# #   21: 'clock'
# #   22: 'fireworks'
# #   23: 'matrix_flow'
# #   24: 'fluid_fill'
# #   25: 'box_pushing'
# #   26: 'lightsaber_duel'
# #   27: 'black_hole'
# #   28: 4
# #   29: 'custom_wave'
# #   30: 'n_colors'
# #   31: 'fire'
# #   32: 'riptide_wave'
# #   33: 3
# #   34: 'gradient'
# #   35: 'interleave'
# #   36: 'meteor'
# #   37: 'sky_cloud'
# #   38: 'camo_fade'
# #   39: 'lasers_lasers'
# #   40: 'storm'
# #   41: 'explode'
# #   42: 'saiyan'
# #   43: 'spiral'
# #   44: 'poison'
# #   45: 'windmill'
# #   46: 'maple_leaf'
# #   47: 'lightsaber_battle'
# #   48: 'police_light'
# #   49: 'sphere_3d'
# #   50: 'dream_dream'
# #   51: 'neon_wave'
# #   52: 'fireflies'
# #   53: 'hellfire'
# #   54: 'dragon_flame'
# #   55: 'hydrogen'
# #   56: 'hyperspace'
# #   57: 'jellyfish'
# #   58: 'koi_pond'
# #   59: 'lasers'
# #   60: 'lava_lamp'
# #   61: 'mario'
# #   62: 'multiverse_simple'
# #   63: 'cosmic_nebula'
# #   64: 'neon_grid'
# #   65: 'neurolink'
# #   66: 'new_years'
# #   67: 'paint_splatters'
# #   68: 'party_parrot'
# #   69: 'pixel_wave'
# #   70: 'planetary_orbit'
# #   71: 'plasma_flow'
# #   72: 'poison_max'
# #   73: 'rainbow_tap'
# #   74: 'resident_evil'
# #   75: 'rick_morty'
# #   76: 1
# #   77: 255
# #   78: '图标取消选择'
# # Names:
# #    0: effect_manager
# #    1: effects
# #    2: current_effect
# #    3: speed_slider
# #    4: setValue
# #    5: speed_label
# #    6: setText
# #    7: speed_label_text
# #    8: show
# #    9: color_btn
# #   10: hide
# #   11: width_slider
# #   12: width_label
# #   13: width_label_txt
# #   14: height_slider
# #   15: height_label
# #   16: height_label_txt
# #   17: color_btns
# #   18: pingjun_radio
# #   19: zhengping_radio
# #   20: str
# #   21: update_color_btn
# #   22: enumerate
# #   23: set_button_color
# #   24: setChecked
# #   25: len
# #   26: canvas_scene
# #   27: selected_icon
# #   28: orientation_group
# #   29: power_group
# #   30: orientation
# #   31: Qt
# #   32: Vertical
# #   33: vertical_radio
# #   34: horizontal_radio
# #   35: power_direction
# #   36: forward_radio
# #   37: reverse_radio
# #   38: device_xuanzhong_flag
# #   39: print
# # Varnames:
# #	self, speed, effect_type, effect, btn, i, color, n_colors
# # Positional arguments:
# #	self
# # Local variables:
# #    1: speed
# #    2: effect_type
# #    3: effect
# #    4: btn
# #    5: i
# #    6: color
# #    7: n_colors
# 13192:           0 LOAD_FAST            (self)
#                2 LOAD_ATTR            (effect_manager)
#                4 LOAD_ATTR            (effects)
#                6 LOAD_FAST            (self)
#                8 LOAD_ATTR            (effect_manager)
#               10 LOAD_ATTR            (current_effect)
#               12 BINARY_SUBSCR
#               14 LOAD_CONST           ("speed")
#               16 BINARY_SUBSCR
#               18 STORE_FAST           (speed)
# 
# 13193:          20 LOAD_FAST            (self)
#               22 LOAD_ATTR            (speed_slider)
#               24 LOAD_METHOD          (setValue)
#               26 LOAD_FAST            (speed)
#               28 CALL_METHOD          (1 positional argument)
#               30 POP_TOP
# 
# 13194:          32 LOAD_FAST            (self)
#               34 LOAD_ATTR            (speed_label)
#               36 LOAD_METHOD          (setText)
#               38 LOAD_FAST            (speed)
#               40 FORMAT_VALUE         0
#               42 CALL_METHOD          (1 positional argument)
#               44 POP_TOP
# 
# 13197:          46 LOAD_FAST            (self)
#               48 LOAD_ATTR            (effect_manager)
#               50 LOAD_ATTR            (current_effect)
#               52 STORE_FAST           (effect_type)
# 
# 13198:          54 LOAD_FAST            (self)
#               56 LOAD_ATTR            (effect_manager)
#               58 LOAD_ATTR            (effects)
#               60 LOAD_FAST            (effect_type)
#               62 BINARY_SUBSCR
#               64 STORE_FAST           (effect)
# 
# 13201:          66 LOAD_FAST            (self)
#               68 LOAD_ATTR            (speed_label_text)
#               70 LOAD_METHOD          (show)
#               72 CALL_METHOD          (0 positional arguments)
#               74 POP_TOP
# 
# 13202:          76 LOAD_FAST            (self)
#               78 LOAD_ATTR            (speed_label)
#               80 LOAD_METHOD          (show)
#               82 CALL_METHOD          (0 positional arguments)
#               84 POP_TOP
# 
# 13203:          86 LOAD_FAST            (self)
#               88 LOAD_ATTR            (speed_slider)
#               90 LOAD_METHOD          (show)
#               92 CALL_METHOD          (0 positional arguments)
#               94 POP_TOP
# 
# 13206:          96 LOAD_FAST            (self)
#               98 LOAD_ATTR            (color_btn)
#              100 LOAD_METHOD          (hide)
#              102 CALL_METHOD          (0 positional arguments)
#              104 POP_TOP
# 
# 13207:         106 LOAD_FAST            (self)
#              108 LOAD_ATTR            (width_slider)
#              110 LOAD_METHOD          (hide)
#              112 CALL_METHOD          (0 positional arguments)
#              114 POP_TOP
# 
# 13208:         116 LOAD_FAST            (self)
#              118 LOAD_ATTR            (width_label)
#              120 LOAD_METHOD          (hide)
#              122 CALL_METHOD          (0 positional arguments)
#              124 POP_TOP
# 
# 13209:         126 LOAD_FAST            (self)
#              128 LOAD_ATTR            (width_label_txt)
#              130 LOAD_METHOD          (hide)
#              132 CALL_METHOD          (0 positional arguments)
#              134 POP_TOP
# 
# 13210:         136 LOAD_FAST            (self)
#              138 LOAD_ATTR            (height_slider)
#              140 LOAD_METHOD          (hide)
#              142 CALL_METHOD          (0 positional arguments)
#              144 POP_TOP
# 
# 13211:         146 LOAD_FAST            (self)
#              148 LOAD_ATTR            (height_label)
#              150 LOAD_METHOD          (hide)
#              152 CALL_METHOD          (0 positional arguments)
#              154 POP_TOP
# 
# 13212:         156 LOAD_FAST            (self)
#              158 LOAD_ATTR            (height_label_txt)
#              160 LOAD_METHOD          (hide)
#              162 CALL_METHOD          (0 positional arguments)
#              164 POP_TOP
# 
# 13213:         166 LOAD_FAST            (self)
#              168 LOAD_ATTR            (color_btns)
#              170 GET_ITER
#          >>  172 FOR_ITER             (to 186)
#              174 STORE_FAST           (btn)
# 
# 13214:         176 LOAD_FAST            (btn)
#              178 LOAD_METHOD          (hide)
#              180 CALL_METHOD          (0 positional arguments)
#              182 POP_TOP
#              184 JUMP_ABSOLUTE        (to 172)
# 
# 13215:     >>  186 LOAD_FAST            (self)
#              188 LOAD_ATTR            (pingjun_radio)
#              190 LOAD_METHOD          (hide)
#              192 CALL_METHOD          (0 positional arguments)
#              194 POP_TOP
# 
# 13216:         196 LOAD_FAST            (self)
#              198 LOAD_ATTR            (zhengping_radio)
#              200 LOAD_METHOD          (hide)
#              202 CALL_METHOD          (0 positional arguments)
#              204 POP_TOP
# 
# 13219:         206 LOAD_FAST            (effect)
#              208 LOAD_CONST           ("type")
#              210 BINARY_SUBSCR
#              212 LOAD_CONST           ("wave")
#              214 COMPARE_OP           (==)
#              216 EXTENDED_ARG         (256)
#              218 POP_JUMP_IF_FALSE    (to 314)
# 
# 13221:         220 LOAD_FAST            (self)
#              222 LOAD_ATTR            (color_btn)
#              224 LOAD_METHOD          (show)
#              226 CALL_METHOD          (0 positional arguments)
#              228 POP_TOP
# 
# 13222:         230 LOAD_FAST            (self)
#              232 LOAD_ATTR            (width_slider)
#              234 LOAD_METHOD          (show)
#              236 CALL_METHOD          (0 positional arguments)
#              238 POP_TOP
# 
# 13223:         240 LOAD_FAST            (self)
#              242 LOAD_ATTR            (width_label)
#              244 LOAD_METHOD          (show)
#              246 CALL_METHOD          (0 positional arguments)
#              248 POP_TOP
# 
# 13224:         250 LOAD_FAST            (self)
#              252 LOAD_ATTR            (width_label_txt)
#              254 LOAD_METHOD          (show)
#              256 CALL_METHOD          (0 positional arguments)
#              258 POP_TOP
# 
# 13227:         260 LOAD_FAST            (self)
#              262 LOAD_ATTR            (width_slider)
#              264 LOAD_METHOD          (setValue)
#              266 LOAD_FAST            (effect)
#              268 LOAD_CONST           ("width")
#              270 BINARY_SUBSCR
#              272 CALL_METHOD          (1 positional argument)
#              274 POP_TOP
# 
# 13228:         276 LOAD_FAST            (self)
#              278 LOAD_ATTR            (width_label_txt)
#              280 LOAD_METHOD          (setText)
#              282 LOAD_GLOBAL          (str)
#              284 LOAD_FAST            (effect)
#              286 LOAD_CONST           ("width")
#              288 BINARY_SUBSCR
#              290 CALL_FUNCTION        (1 positional argument)
#              292 CALL_METHOD          (1 positional argument)
#              294 POP_TOP
# 
# 13229:         296 LOAD_FAST            (self)
#              298 LOAD_METHOD          (update_color_btn)
#              300 LOAD_FAST            (effect)
#              302 LOAD_CONST           ("color")
#              304 BINARY_SUBSCR
#              306 CALL_METHOD          (1 positional argument)
#              308 POP_TOP
#              310 EXTENDED_ARG         (5120)
#              312 JUMP_FORWARD         (to 5556)
# 
# 13231:     >>  314 LOAD_FAST            (effect)
#              316 LOAD_CONST           ("type")
#              318 BINARY_SUBSCR
#              320 LOAD_CONST           ("block")
#              322 COMPARE_OP           (==)
#              324 EXTENDED_ARG         (256)
#              326 POP_JUMP_IF_FALSE    (to 422)
# 
# 13233:         328 LOAD_FAST            (self)
#              330 LOAD_ATTR            (color_btn)
#              332 LOAD_METHOD          (show)
#              334 CALL_METHOD          (0 positional arguments)
#              336 POP_TOP
# 
# 13234:         338 LOAD_FAST            (self)
#              340 LOAD_ATTR            (height_slider)
#              342 LOAD_METHOD          (show)
#              344 CALL_METHOD          (0 positional arguments)
#              346 POP_TOP
# 
# 13235:         348 LOAD_FAST            (self)
#              350 LOAD_ATTR            (height_label)
#              352 LOAD_METHOD          (show)
#              354 CALL_METHOD          (0 positional arguments)
#              356 POP_TOP
# 
# 13236:         358 LOAD_FAST            (self)
#              360 LOAD_ATTR            (height_label_txt)
#              362 LOAD_METHOD          (show)
#              364 CALL_METHOD          (0 positional arguments)
#              366 POP_TOP
# 
# 13239:         368 LOAD_FAST            (self)
#              370 LOAD_ATTR            (height_slider)
#              372 LOAD_METHOD          (setValue)
#              374 LOAD_FAST            (effect)
#              376 LOAD_CONST           ("height")
#              378 BINARY_SUBSCR
#              380 CALL_METHOD          (1 positional argument)
#              382 POP_TOP
# 
# 13240:         384 LOAD_FAST            (self)
#              386 LOAD_ATTR            (height_label_txt)
#              388 LOAD_METHOD          (setText)
#              390 LOAD_GLOBAL          (str)
#              392 LOAD_FAST            (effect)
#              394 LOAD_CONST           ("height")
#              396 BINARY_SUBSCR
#              398 CALL_FUNCTION        (1 positional argument)
#              400 CALL_METHOD          (1 positional argument)
#              402 POP_TOP
# 
# 13241:         404 LOAD_FAST            (self)
#              406 LOAD_METHOD          (update_color_btn)
#              408 LOAD_FAST            (effect)
#              410 LOAD_CONST           ("color")
#              412 BINARY_SUBSCR
#              414 CALL_METHOD          (1 positional argument)
#              416 POP_TOP
#              418 EXTENDED_ARG         (5120)
#              420 JUMP_FORWARD         (to 5556)
# 
# 13243:     >>  422 LOAD_FAST            (effect)
#              424 LOAD_CONST           ("type")
#              426 BINARY_SUBSCR
#              428 LOAD_CONST           ("four_directions")
#              430 COMPARE_OP           (==)
#              432 EXTENDED_ARG         (256)
#              434 POP_JUMP_IF_FALSE    (to 494)
# 
# 13245:         436 LOAD_GLOBAL          (enumerate)
#              438 LOAD_FAST            (self)
#              440 LOAD_ATTR            (color_btns)
#              442 CALL_FUNCTION        (1 positional argument)
#              444 GET_ITER
#          >>  446 FOR_ITER             (to 490)
#              448 UNPACK_SEQUENCE      2
#              450 STORE_FAST           (i)
#              452 STORE_FAST           (btn)
# 
# 13246:         454 LOAD_FAST            (btn)
#              456 LOAD_METHOD          (show)
#              458 CALL_METHOD          (0 positional arguments)
#              460 POP_TOP
# 
# 13248:         462 LOAD_FAST            (effect)
#              464 LOAD_CONST           ("colors")
#              466 BINARY_SUBSCR
#              468 LOAD_FAST            (i)
#              470 BINARY_SUBSCR
#              472 STORE_FAST           (color)
# 
# 13249:         474 LOAD_FAST            (self)
#              476 LOAD_METHOD          (set_button_color)
#              478 LOAD_FAST            (btn)
#              480 LOAD_FAST            (color)
#              482 CALL_METHOD          (2 positional arguments)
#              484 POP_TOP
#              486 EXTENDED_ARG         (256)
#              488 JUMP_ABSOLUTE        (to 446)
#          >>  490 EXTENDED_ARG         (4864)
#              492 JUMP_FORWARD         (to 5556)
# 
# 13251:     >>  494 LOAD_FAST            (effect)
#              496 LOAD_CONST           ("type")
#              498 BINARY_SUBSCR
#              500 LOAD_CONST           ("corner_fill")
#              502 COMPARE_OP           (==)
#              504 EXTENDED_ARG         (512)
#              506 POP_JUMP_IF_FALSE    (to 566)
# 
# 13253:         508 LOAD_GLOBAL          (enumerate)
#              510 LOAD_FAST            (self)
#              512 LOAD_ATTR            (color_btns)
#              514 CALL_FUNCTION        (1 positional argument)
#              516 GET_ITER
#          >>  518 FOR_ITER             (to 562)
#              520 UNPACK_SEQUENCE      2
#              522 STORE_FAST           (i)
#              524 STORE_FAST           (btn)
# 
# 13254:         526 LOAD_FAST            (btn)
#              528 LOAD_METHOD          (show)
#              530 CALL_METHOD          (0 positional arguments)
#              532 POP_TOP
# 
# 13256:         534 LOAD_FAST            (effect)
#              536 LOAD_CONST           ("colors")
#              538 BINARY_SUBSCR
#              540 LOAD_FAST            (i)
#              542 BINARY_SUBSCR
#              544 STORE_FAST           (color)
# 
# 13257:         546 LOAD_FAST            (self)
#              548 LOAD_METHOD          (set_button_color)
#              550 LOAD_FAST            (btn)
#              552 LOAD_FAST            (color)
#              554 CALL_METHOD          (2 positional arguments)
#              556 POP_TOP
#              558 EXTENDED_ARG         (512)
#              560 JUMP_ABSOLUTE        (to 518)
#          >>  562 EXTENDED_ARG         (4864)
#              564 JUMP_FORWARD         (to 5556)
# 
# 13258:     >>  566 LOAD_FAST            (effect)
#              568 LOAD_CONST           ("type")
#              570 BINARY_SUBSCR
#              572 LOAD_CONST           ("bubble")
#              574 COMPARE_OP           (==)
#              576 EXTENDED_ARG         (512)
#              578 POP_JUMP_IF_FALSE    (to 648)
# 
# 13263:         580 LOAD_GLOBAL          (enumerate)
#              582 LOAD_FAST            (self)
#              584 LOAD_ATTR            (color_btns)
#              586 CALL_FUNCTION        (1 positional argument)
#              588 GET_ITER
#          >>  590 FOR_ITER             (to 644)
#              592 UNPACK_SEQUENCE      2
#              594 STORE_FAST           (i)
#              596 STORE_FAST           (btn)
# 
# 13264:         598 LOAD_FAST            (i)
#              600 LOAD_CONST           (2)
#              602 COMPARE_OP           (<)
#              604 EXTENDED_ARG         (512)
#              606 POP_JUMP_IF_FALSE    (to 590)
# 
# 13265:         608 LOAD_FAST            (btn)
#              610 LOAD_METHOD          (show)
#              612 CALL_METHOD          (0 positional arguments)
#              614 POP_TOP
# 
# 13267:         616 LOAD_FAST            (effect)
#              618 LOAD_CONST           ("colors")
#              620 BINARY_SUBSCR
#              622 LOAD_FAST            (i)
#              624 BINARY_SUBSCR
#              626 STORE_FAST           (color)
# 
# 13268:         628 LOAD_FAST            (self)
#              630 LOAD_METHOD          (set_button_color)
#              632 LOAD_FAST            (btn)
#              634 LOAD_FAST            (color)
#              636 CALL_METHOD          (2 positional arguments)
#              638 POP_TOP
#              640 EXTENDED_ARG         (512)
#              642 JUMP_ABSOLUTE        (to 590)
#          >>  644 EXTENDED_ARG         (4864)
#              646 JUMP_FORWARD         (to 5556)
# 
# 13269:     >>  648 LOAD_FAST            (effect)
#              650 LOAD_CONST           ("type")
#              652 BINARY_SUBSCR
#              654 LOAD_CONST           ("dna_helix")
#              656 COMPARE_OP           (==)
#              658 EXTENDED_ARG         (512)
#              660 POP_JUMP_IF_FALSE    (to 690)
# 
# 13271:         662 LOAD_FAST            (self)
#              664 LOAD_ATTR            (color_btn)
#              666 LOAD_METHOD          (show)
#              668 CALL_METHOD          (0 positional arguments)
#              670 POP_TOP
# 
# 13273:         672 LOAD_FAST            (self)
#              674 LOAD_METHOD          (update_color_btn)
#              676 LOAD_FAST            (effect)
#              678 LOAD_CONST           ("color")
#              680 BINARY_SUBSCR
#              682 CALL_METHOD          (1 positional argument)
#              684 POP_TOP
#              686 EXTENDED_ARG         (4864)
#              688 JUMP_FORWARD         (to 5556)
# 
# 13274:     >>  690 LOAD_FAST            (effect)
#              692 LOAD_CONST           ("type")
#              694 BINARY_SUBSCR
#              696 LOAD_CONST           ("radar")
#              698 COMPARE_OP           (==)
#              700 EXTENDED_ARG         (512)
#              702 POP_JUMP_IF_FALSE    (to 762)
# 
# 13276:         704 LOAD_GLOBAL          (enumerate)
#              706 LOAD_FAST            (self)
#              708 LOAD_ATTR            (color_btns)
#              710 CALL_FUNCTION        (1 positional argument)
#              712 GET_ITER
#          >>  714 FOR_ITER             (to 758)
#              716 UNPACK_SEQUENCE      2
#              718 STORE_FAST           (i)
#              720 STORE_FAST           (btn)
# 
# 13277:         722 LOAD_FAST            (btn)
#              724 LOAD_METHOD          (show)
#              726 CALL_METHOD          (0 positional arguments)
#              728 POP_TOP
# 
# 13279:         730 LOAD_FAST            (effect)
#              732 LOAD_CONST           ("colors")
#              734 BINARY_SUBSCR
#              736 LOAD_FAST            (i)
#              738 BINARY_SUBSCR
#              740 STORE_FAST           (color)
# 
# 13280:         742 LOAD_FAST            (self)
#              744 LOAD_METHOD          (set_button_color)
#              746 LOAD_FAST            (btn)
#              748 LOAD_FAST            (color)
#              750 CALL_METHOD          (2 positional arguments)
#              752 POP_TOP
#              754 EXTENDED_ARG         (512)
#              756 JUMP_ABSOLUTE        (to 714)
#          >>  758 EXTENDED_ARG         (4608)
#              760 JUMP_FORWARD         (to 5556)
# 
# 13281:     >>  762 LOAD_FAST            (effect)
#              764 LOAD_CONST           ("type")
#              766 BINARY_SUBSCR
#              768 LOAD_CONST           ("diagonal_visualizer")
#              770 COMPARE_OP           (==)
#              772 EXTENDED_ARG         (768)
#              774 POP_JUMP_IF_FALSE    (to 804)
# 
# 13283:         776 LOAD_FAST            (self)
#              778 LOAD_ATTR            (color_btn)
#              780 LOAD_METHOD          (show)
#              782 CALL_METHOD          (0 positional arguments)
#              784 POP_TOP
# 
# 13285:         786 LOAD_FAST            (self)
#              788 LOAD_METHOD          (update_color_btn)
#              790 LOAD_FAST            (effect)
#              792 LOAD_CONST           ("color")
#              794 BINARY_SUBSCR
#              796 CALL_METHOD          (1 positional argument)
#              798 POP_TOP
#              800 EXTENDED_ARG         (4608)
#              802 JUMP_FORWARD         (to 5556)
# 
# 13286:     >>  804 LOAD_FAST            (effect)
#              806 LOAD_CONST           ("type")
#              808 BINARY_SUBSCR
#              810 LOAD_CONST           ("music_center_side")
#              812 COMPARE_OP           (==)
#              814 EXTENDED_ARG         (768)
#              816 POP_JUMP_IF_FALSE    (to 876)
# 
# 13288:         818 LOAD_FAST            (self)
#              820 LOAD_ATTR            (speed_label_text)
#              822 LOAD_METHOD          (hide)
#              824 CALL_METHOD          (0 positional arguments)
#              826 POP_TOP
# 
# 13289:         828 LOAD_FAST            (self)
#              830 LOAD_ATTR            (speed_label)
#              832 LOAD_METHOD          (hide)
#              834 CALL_METHOD          (0 positional arguments)
#              836 POP_TOP
# 
# 13290:         838 LOAD_FAST            (self)
#              840 LOAD_ATTR            (speed_slider)
#              842 LOAD_METHOD          (hide)
#              844 CALL_METHOD          (0 positional arguments)
#              846 POP_TOP
# 
# 13292:         848 LOAD_FAST            (self)
#              850 LOAD_ATTR            (color_btn)
#              852 LOAD_METHOD          (show)
#              854 CALL_METHOD          (0 positional arguments)
#              856 POP_TOP
# 
# 13294:         858 LOAD_FAST            (self)
#              860 LOAD_METHOD          (update_color_btn)
#              862 LOAD_FAST            (effect)
#              864 LOAD_CONST           ("color")
#              866 BINARY_SUBSCR
#              868 CALL_METHOD          (1 positional argument)
#              870 POP_TOP
#              872 EXTENDED_ARG         (4608)
#              874 JUMP_FORWARD         (to 5556)
# 
# 13296:     >>  876 LOAD_FAST            (effect)
#              878 LOAD_CONST           ("type")
#              880 BINARY_SUBSCR
#              882 LOAD_CONST           ("screen_effects")
#              884 COMPARE_OP           (==)
#              886 EXTENDED_ARG         (768)
#              888 POP_JUMP_IF_FALSE    (to 984)
# 
# 13298:         890 LOAD_FAST            (self)
#              892 LOAD_ATTR            (speed_label_text)
#              894 LOAD_METHOD          (hide)
#              896 CALL_METHOD          (0 positional arguments)
#              898 POP_TOP
# 
# 13299:         900 LOAD_FAST            (self)
#              902 LOAD_ATTR            (speed_label)
#              904 LOAD_METHOD          (hide)
#              906 CALL_METHOD          (0 positional arguments)
#              908 POP_TOP
# 
# 13300:         910 LOAD_FAST            (self)
#              912 LOAD_ATTR            (speed_slider)
#              914 LOAD_METHOD          (hide)
#              916 CALL_METHOD          (0 positional arguments)
#              918 POP_TOP
# 
# 13303:         920 LOAD_FAST            (self)
#              922 LOAD_ATTR            (pingjun_radio)
#              924 LOAD_METHOD          (show)
#              926 CALL_METHOD          (0 positional arguments)
#              928 POP_TOP
# 
# 13304:         930 LOAD_FAST            (self)
#              932 LOAD_ATTR            (zhengping_radio)
#              934 LOAD_METHOD          (show)
#              936 CALL_METHOD          (0 positional arguments)
#              938 POP_TOP
# 
# 13307:         940 LOAD_FAST            (effect)
#              942 LOAD_CONST           ("quse")
#              944 BINARY_SUBSCR
#              946 LOAD_CONST           (0)
#              948 COMPARE_OP           (==)
#              950 EXTENDED_ARG         (768)
#              952 POP_JUMP_IF_FALSE    (to 968)
# 
# 13308:         954 LOAD_FAST            (self)
#              956 LOAD_ATTR            (pingjun_radio)
#              958 LOAD_METHOD          (setChecked)
#              960 LOAD_CONST           (True)
#              962 CALL_METHOD          (1 positional argument)
#              964 POP_TOP
#              966 JUMP_FORWARD         (to 980)
# 
# 13310:     >>  968 LOAD_FAST            (self)
#              970 LOAD_ATTR            (zhengping_radio)
#              972 LOAD_METHOD          (setChecked)
#              974 LOAD_CONST           (True)
#              976 CALL_METHOD          (1 positional argument)
#              978 POP_TOP
#          >>  980 EXTENDED_ARG         (4352)
#              982 JUMP_FORWARD         (to 5556)
# 
# 13311:     >>  984 LOAD_FAST            (effect)
#              986 LOAD_CONST           ("type")
#              988 BINARY_SUBSCR
#              990 LOAD_CONST           ("clock")
#              992 COMPARE_OP           (==)
#              994 EXTENDED_ARG         (1024)
#              996 POP_JUMP_IF_FALSE    (to 1086)
# 
# 13313:         998 LOAD_FAST            (self)
#             1000 LOAD_ATTR            (speed_label_text)
#             1002 LOAD_METHOD          (hide)
#             1004 CALL_METHOD          (0 positional arguments)
#             1006 POP_TOP
# 
# 13314:        1008 LOAD_FAST            (self)
#             1010 LOAD_ATTR            (speed_label)
#             1012 LOAD_METHOD          (hide)
#             1014 CALL_METHOD          (0 positional arguments)
#             1016 POP_TOP
# 
# 13315:        1018 LOAD_FAST            (self)
#             1020 LOAD_ATTR            (speed_slider)
#             1022 LOAD_METHOD          (hide)
#             1024 CALL_METHOD          (0 positional arguments)
#             1026 POP_TOP
# 
# 13318:        1028 LOAD_GLOBAL          (enumerate)
#             1030 LOAD_FAST            (self)
#             1032 LOAD_ATTR            (color_btns)
#             1034 CALL_FUNCTION        (1 positional argument)
#             1036 GET_ITER
#          >> 1038 FOR_ITER             (to 1082)
#             1040 UNPACK_SEQUENCE      2
#             1042 STORE_FAST           (i)
#             1044 STORE_FAST           (btn)
# 
# 13319:        1046 LOAD_FAST            (btn)
#             1048 LOAD_METHOD          (show)
#             1050 CALL_METHOD          (0 positional arguments)
#             1052 POP_TOP
# 
# 13321:        1054 LOAD_FAST            (effect)
#             1056 LOAD_CONST           ("colors")
#             1058 BINARY_SUBSCR
#             1060 LOAD_FAST            (i)
#             1062 BINARY_SUBSCR
#             1064 STORE_FAST           (color)
# 
# 13322:        1066 LOAD_FAST            (self)
#             1068 LOAD_METHOD          (set_button_color)
#             1070 LOAD_FAST            (btn)
#             1072 LOAD_FAST            (color)
#             1074 CALL_METHOD          (2 positional arguments)
#             1076 POP_TOP
#             1078 EXTENDED_ARG         (1024)
#             1080 JUMP_ABSOLUTE        (to 1038)
#          >> 1082 EXTENDED_ARG         (4352)
#             1084 JUMP_FORWARD         (to 5556)
# 
# 13323:     >> 1086 LOAD_FAST            (effect)
#             1088 LOAD_CONST           ("type")
#             1090 BINARY_SUBSCR
#             1092 LOAD_CONST           ("fireworks")
#             1094 COMPARE_OP           (==)
#             1096 EXTENDED_ARG         (1024)
#             1098 POP_JUMP_IF_FALSE    (to 1168)
# 
# 13325:        1100 LOAD_GLOBAL          (enumerate)
#             1102 LOAD_FAST            (self)
#             1104 LOAD_ATTR            (color_btns)
#             1106 CALL_FUNCTION        (1 positional argument)
#             1108 GET_ITER
#          >> 1110 FOR_ITER             (to 1164)
#             1112 UNPACK_SEQUENCE      2
#             1114 STORE_FAST           (i)
#             1116 STORE_FAST           (btn)
# 
# 13326:        1118 LOAD_FAST            (i)
#             1120 LOAD_CONST           (2)
#             1122 COMPARE_OP           (<)
#             1124 EXTENDED_ARG         (1024)
#             1126 POP_JUMP_IF_FALSE    (to 1110)
# 
# 13327:        1128 LOAD_FAST            (btn)
#             1130 LOAD_METHOD          (show)
#             1132 CALL_METHOD          (0 positional arguments)
#             1134 POP_TOP
# 
# 13329:        1136 LOAD_FAST            (effect)
#             1138 LOAD_CONST           ("colors")
#             1140 BINARY_SUBSCR
#             1142 LOAD_FAST            (i)
#             1144 BINARY_SUBSCR
#             1146 STORE_FAST           (color)
# 
# 13330:        1148 LOAD_FAST            (self)
#             1150 LOAD_METHOD          (set_button_color)
#             1152 LOAD_FAST            (btn)
#             1154 LOAD_FAST            (color)
#             1156 CALL_METHOD          (2 positional arguments)
#             1158 POP_TOP
#             1160 EXTENDED_ARG         (1024)
#             1162 JUMP_ABSOLUTE        (to 1110)
#          >> 1164 EXTENDED_ARG         (4352)
#             1166 JUMP_FORWARD         (to 5556)
# 
# 13331:     >> 1168 LOAD_FAST            (effect)
#             1170 LOAD_CONST           ("type")
#             1172 BINARY_SUBSCR
#             1174 LOAD_CONST           ("matrix_flow")
#             1176 COMPARE_OP           (==)
#             1178 EXTENDED_ARG         (1024)
#             1180 POP_JUMP_IF_FALSE    (to 1250)
# 
# 13333:        1182 LOAD_GLOBAL          (enumerate)
#             1184 LOAD_FAST            (self)
#             1186 LOAD_ATTR            (color_btns)
#             1188 CALL_FUNCTION        (1 positional argument)
#             1190 GET_ITER
#          >> 1192 FOR_ITER             (to 1246)
#             1194 UNPACK_SEQUENCE      2
#             1196 STORE_FAST           (i)
#             1198 STORE_FAST           (btn)
# 
# 13334:        1200 LOAD_FAST            (i)
#             1202 LOAD_CONST           (2)
#             1204 COMPARE_OP           (<)
#             1206 EXTENDED_ARG         (1024)
#             1208 POP_JUMP_IF_FALSE    (to 1192)
# 
# 13335:        1210 LOAD_FAST            (btn)
#             1212 LOAD_METHOD          (show)
#             1214 CALL_METHOD          (0 positional arguments)
#             1216 POP_TOP
# 
# 13337:        1218 LOAD_FAST            (effect)
#             1220 LOAD_CONST           ("colors")
#             1222 BINARY_SUBSCR
#             1224 LOAD_FAST            (i)
#             1226 BINARY_SUBSCR
#             1228 STORE_FAST           (color)
# 
# 13338:        1230 LOAD_FAST            (self)
#             1232 LOAD_METHOD          (set_button_color)
#             1234 LOAD_FAST            (btn)
#             1236 LOAD_FAST            (color)
#             1238 CALL_METHOD          (2 positional arguments)
#             1240 POP_TOP
#             1242 EXTENDED_ARG         (1024)
#             1244 JUMP_ABSOLUTE        (to 1192)
#          >> 1246 EXTENDED_ARG         (4096)
#             1248 JUMP_FORWARD         (to 5556)
# 
# 13340:     >> 1250 LOAD_FAST            (effect)
#             1252 LOAD_CONST           ("type")
#             1254 BINARY_SUBSCR
#             1256 LOAD_CONST           ("fluid_fill")
#             1258 COMPARE_OP           (==)
#             1260 EXTENDED_ARG         (1280)
#             1262 POP_JUMP_IF_FALSE    (to 1292)
# 
# 13342:        1264 LOAD_FAST            (self)
#             1266 LOAD_ATTR            (color_btn)
#             1268 LOAD_METHOD          (show)
#             1270 CALL_METHOD          (0 positional arguments)
#             1272 POP_TOP
# 
# 13344:        1274 LOAD_FAST            (self)
#             1276 LOAD_METHOD          (update_color_btn)
#             1278 LOAD_FAST            (effect)
#             1280 LOAD_CONST           ("color")
#             1282 BINARY_SUBSCR
#             1284 CALL_METHOD          (1 positional argument)
#             1286 POP_TOP
#             1288 EXTENDED_ARG         (4096)
#             1290 JUMP_FORWARD         (to 5556)
# 
# 13345:     >> 1292 LOAD_FAST            (effect)
#             1294 LOAD_CONST           ("type")
#             1296 BINARY_SUBSCR
#             1298 LOAD_CONST           ("box_pushing")
#             1300 COMPARE_OP           (==)
#             1302 EXTENDED_ARG         (1280)
#             1304 POP_JUMP_IF_FALSE    (to 1334)
# 
# 13347:        1306 LOAD_FAST            (self)
#             1308 LOAD_ATTR            (color_btn)
#             1310 LOAD_METHOD          (show)
#             1312 CALL_METHOD          (0 positional arguments)
#             1314 POP_TOP
# 
# 13349:        1316 LOAD_FAST            (self)
#             1318 LOAD_METHOD          (update_color_btn)
#             1320 LOAD_FAST            (effect)
#             1322 LOAD_CONST           ("color")
#             1324 BINARY_SUBSCR
#             1326 CALL_METHOD          (1 positional argument)
#             1328 POP_TOP
#             1330 EXTENDED_ARG         (4096)
#             1332 JUMP_FORWARD         (to 5556)
# 
# 13351:     >> 1334 LOAD_FAST            (effect)
#             1336 LOAD_CONST           ("type")
#             1338 BINARY_SUBSCR
#             1340 LOAD_CONST           ("lightsaber_duel")
#             1342 COMPARE_OP           (==)
#             1344 EXTENDED_ARG         (1280)
#             1346 POP_JUMP_IF_FALSE    (to 1416)
# 
# 13353:        1348 LOAD_GLOBAL          (enumerate)
#             1350 LOAD_FAST            (self)
#             1352 LOAD_ATTR            (color_btns)
#             1354 CALL_FUNCTION        (1 positional argument)
#             1356 GET_ITER
#          >> 1358 FOR_ITER             (to 1412)
#             1360 UNPACK_SEQUENCE      2
#             1362 STORE_FAST           (i)
#             1364 STORE_FAST           (btn)
# 
# 13354:        1366 LOAD_FAST            (i)
#             1368 LOAD_CONST           (2)
#             1370 COMPARE_OP           (<)
#             1372 EXTENDED_ARG         (1280)
#             1374 POP_JUMP_IF_FALSE    (to 1358)
# 
# 13355:        1376 LOAD_FAST            (btn)
#             1378 LOAD_METHOD          (show)
#             1380 CALL_METHOD          (0 positional arguments)
#             1382 POP_TOP
# 
# 13357:        1384 LOAD_FAST            (effect)
#             1386 LOAD_CONST           ("colors")
#             1388 BINARY_SUBSCR
#             1390 LOAD_FAST            (i)
#             1392 BINARY_SUBSCR
#             1394 STORE_FAST           (color)
# 
# 13358:        1396 LOAD_FAST            (self)
#             1398 LOAD_METHOD          (set_button_color)
#             1400 LOAD_FAST            (btn)
#             1402 LOAD_FAST            (color)
#             1404 CALL_METHOD          (2 positional arguments)
#             1406 POP_TOP
#             1408 EXTENDED_ARG         (1280)
#             1410 JUMP_ABSOLUTE        (to 1358)
#          >> 1412 EXTENDED_ARG         (4096)
#             1414 JUMP_FORWARD         (to 5556)
# 
# 13359:     >> 1416 LOAD_FAST            (effect)
#             1418 LOAD_CONST           ("type")
#             1420 BINARY_SUBSCR
#             1422 LOAD_CONST           ("black_hole")
#             1424 COMPARE_OP           (==)
#             1426 EXTENDED_ARG         (1280)
#             1428 POP_JUMP_IF_FALSE    (to 1508)
# 
# 13361:        1430 LOAD_GLOBAL          (enumerate)
#             1432 LOAD_FAST            (self)
#             1434 LOAD_ATTR            (color_btns)
#             1436 CALL_FUNCTION        (1 positional argument)
#             1438 GET_ITER
#          >> 1440 FOR_ITER             (to 1504)
#             1442 UNPACK_SEQUENCE      2
#             1444 STORE_FAST           (i)
#             1446 STORE_FAST           (btn)
# 
# 13362:        1448 LOAD_FAST            (i)
#             1450 LOAD_CONST           (4)
#             1452 COMPARE_OP           (<)
#             1454 EXTENDED_ARG         (1280)
#             1456 POP_JUMP_IF_FALSE    (to 1492)
# 
# 13363:        1458 LOAD_FAST            (btn)
#             1460 LOAD_METHOD          (show)
#             1462 CALL_METHOD          (0 positional arguments)
#             1464 POP_TOP
# 
# 13364:        1466 LOAD_FAST            (effect)
#             1468 LOAD_CONST           ("colors")
#             1470 BINARY_SUBSCR
#             1472 LOAD_FAST            (i)
#             1474 BINARY_SUBSCR
#             1476 STORE_FAST           (color)
# 
# 13365:        1478 LOAD_FAST            (self)
#             1480 LOAD_METHOD          (set_button_color)
#             1482 LOAD_FAST            (btn)
#             1484 LOAD_FAST            (color)
#             1486 CALL_METHOD          (2 positional arguments)
#             1488 POP_TOP
#             1490 JUMP_FORWARD         (to 1500)
# 
# 13367:     >> 1492 LOAD_FAST            (btn)
#             1494 LOAD_METHOD          (hide)
#             1496 CALL_METHOD          (0 positional arguments)
#             1498 POP_TOP
#          >> 1500 EXTENDED_ARG         (1280)
#             1502 JUMP_ABSOLUTE        (to 1440)
#          >> 1504 EXTENDED_ARG         (3840)
#             1506 JUMP_FORWARD         (to 5556)
# 
# 13368:     >> 1508 LOAD_FAST            (effect)
#             1510 LOAD_CONST           ("type")
#             1512 BINARY_SUBSCR
#             1514 LOAD_CONST           ("custom_wave")
#             1516 COMPARE_OP           (==)
#             1518 EXTENDED_ARG         (1536)
#             1520 POP_JUMP_IF_FALSE    (to 1608)
# 
# 13369:        1522 LOAD_FAST            (effect)
#             1524 LOAD_CONST           ("n_colors")
#             1526 BINARY_SUBSCR
#             1528 STORE_FAST           (n_colors)
# 
# 13370:        1530 LOAD_GLOBAL          (enumerate)
#             1532 LOAD_FAST            (self)
#             1534 LOAD_ATTR            (color_btns)
#             1536 CALL_FUNCTION        (1 positional argument)
#             1538 GET_ITER
#          >> 1540 FOR_ITER             (to 1604)
#             1542 UNPACK_SEQUENCE      2
#             1544 STORE_FAST           (i)
#             1546 STORE_FAST           (btn)
# 
# 13371:        1548 LOAD_FAST            (i)
#             1550 LOAD_FAST            (n_colors)
#             1552 COMPARE_OP           (<)
#             1554 EXTENDED_ARG         (1536)
#             1556 POP_JUMP_IF_FALSE    (to 1592)
# 
# 13372:        1558 LOAD_FAST            (btn)
#             1560 LOAD_METHOD          (show)
#             1562 CALL_METHOD          (0 positional arguments)
#             1564 POP_TOP
# 
# 13373:        1566 LOAD_FAST            (effect)
#             1568 LOAD_CONST           ("colors")
#             1570 BINARY_SUBSCR
#             1572 LOAD_FAST            (i)
#             1574 BINARY_SUBSCR
#             1576 STORE_FAST           (color)
# 
# 13374:        1578 LOAD_FAST            (self)
#             1580 LOAD_METHOD          (set_button_color)
#             1582 LOAD_FAST            (btn)
#             1584 LOAD_FAST            (color)
#             1586 CALL_METHOD          (2 positional arguments)
#             1588 POP_TOP
#             1590 JUMP_FORWARD         (to 1600)
# 
# 13376:     >> 1592 LOAD_FAST            (btn)
#             1594 LOAD_METHOD          (hide)
#             1596 CALL_METHOD          (0 positional arguments)
#             1598 POP_TOP
#          >> 1600 EXTENDED_ARG         (1536)
#             1602 JUMP_ABSOLUTE        (to 1540)
#          >> 1604 EXTENDED_ARG         (3840)
#             1606 JUMP_FORWARD         (to 5556)
# 
# 13377:     >> 1608 LOAD_FAST            (effect)
#             1610 LOAD_CONST           ("type")
#             1612 BINARY_SUBSCR
#             1614 LOAD_CONST           ("fire")
#             1616 COMPARE_OP           (==)
#             1618 EXTENDED_ARG         (1536)
#             1620 POP_JUMP_IF_FALSE    (to 1696)
# 
# 13379:        1622 LOAD_GLOBAL          (enumerate)
#             1624 LOAD_FAST            (self)
#             1626 LOAD_ATTR            (color_btns)
#             1628 CALL_FUNCTION        (1 positional argument)
#             1630 GET_ITER
#          >> 1632 FOR_ITER             (to 1692)
#             1634 UNPACK_SEQUENCE      2
#             1636 STORE_FAST           (i)
#             1638 STORE_FAST           (btn)
# 
# 13380:        1640 LOAD_FAST            (i)
#             1642 LOAD_CONST           (4)
#             1644 COMPARE_OP           (<)
#             1646 EXTENDED_ARG         (1536)
#             1648 POP_JUMP_IF_FALSE    (to 1680)
# 
# 13381:        1650 LOAD_FAST            (btn)
#             1652 LOAD_METHOD          (show)
#             1654 CALL_METHOD          (0 positional arguments)
#             1656 POP_TOP
# 
# 13382:        1658 LOAD_FAST            (self)
#             1660 LOAD_METHOD          (set_button_color)
#             1662 LOAD_FAST            (btn)
#             1664 LOAD_FAST            (effect)
#             1666 LOAD_CONST           ("colors")
#             1668 BINARY_SUBSCR
#             1670 LOAD_FAST            (i)
#             1672 BINARY_SUBSCR
#             1674 CALL_METHOD          (2 positional arguments)
#             1676 POP_TOP
#             1678 JUMP_FORWARD         (to 1688)
# 
# 13384:     >> 1680 LOAD_FAST            (btn)
#             1682 LOAD_METHOD          (hide)
#             1684 CALL_METHOD          (0 positional arguments)
#             1686 POP_TOP
#          >> 1688 EXTENDED_ARG         (1536)
#             1690 JUMP_ABSOLUTE        (to 1632)
#          >> 1692 EXTENDED_ARG         (3840)
#             1694 JUMP_FORWARD         (to 5556)
# 
# 13385:     >> 1696 LOAD_FAST            (effect)
#             1698 LOAD_CONST           ("type")
#             1700 BINARY_SUBSCR
#             1702 LOAD_CONST           ("riptide_wave")
#             1704 COMPARE_OP           (==)
#             1706 EXTENDED_ARG         (1536)
#             1708 POP_JUMP_IF_FALSE    (to 1784)
# 
# 13387:        1710 LOAD_GLOBAL          (enumerate)
#             1712 LOAD_FAST            (self)
#             1714 LOAD_ATTR            (color_btns)
#             1716 CALL_FUNCTION        (1 positional argument)
#             1718 GET_ITER
#          >> 1720 FOR_ITER             (to 1780)
#             1722 UNPACK_SEQUENCE      2
#             1724 STORE_FAST           (i)
#             1726 STORE_FAST           (btn)
# 
# 13388:        1728 LOAD_FAST            (i)
#             1730 LOAD_CONST           (3)
#             1732 COMPARE_OP           (<)
#             1734 EXTENDED_ARG         (1536)
#             1736 POP_JUMP_IF_FALSE    (to 1768)
# 
# 13389:        1738 LOAD_FAST            (btn)
#             1740 LOAD_METHOD          (show)
#             1742 CALL_METHOD          (0 positional arguments)
#             1744 POP_TOP
# 
# 13390:        1746 LOAD_FAST            (self)
#             1748 LOAD_METHOD          (set_button_color)
#             1750 LOAD_FAST            (btn)
#             1752 LOAD_FAST            (effect)
#             1754 LOAD_CONST           ("colors")
#             1756 BINARY_SUBSCR
#             1758 LOAD_FAST            (i)
#             1760 BINARY_SUBSCR
#             1762 CALL_METHOD          (2 positional arguments)
#             1764 POP_TOP
#             1766 JUMP_FORWARD         (to 1776)
# 
# 13392:     >> 1768 LOAD_FAST            (btn)
#             1770 LOAD_METHOD          (hide)
#             1772 CALL_METHOD          (0 positional arguments)
#             1774 POP_TOP
#          >> 1776 EXTENDED_ARG         (1536)
#             1778 JUMP_ABSOLUTE        (to 1720)
#          >> 1780 EXTENDED_ARG         (3584)
#             1782 JUMP_FORWARD         (to 5556)
# 
# 13393:     >> 1784 LOAD_FAST            (effect)
#             1786 LOAD_CONST           ("type")
#             1788 BINARY_SUBSCR
#             1790 LOAD_CONST           ("gradient")
#             1792 COMPARE_OP           (==)
#             1794 EXTENDED_ARG         (1792)
#             1796 POP_JUMP_IF_FALSE    (to 1872)
# 
# 13395:        1798 LOAD_GLOBAL          (enumerate)
#             1800 LOAD_FAST            (self)
#             1802 LOAD_ATTR            (color_btns)
#             1804 CALL_FUNCTION        (1 positional argument)
#             1806 GET_ITER
#          >> 1808 FOR_ITER             (to 1868)
#             1810 UNPACK_SEQUENCE      2
#             1812 STORE_FAST           (i)
#             1814 STORE_FAST           (btn)
# 
# 13396:        1816 LOAD_FAST            (i)
#             1818 LOAD_CONST           (4)
#             1820 COMPARE_OP           (<)
#             1822 EXTENDED_ARG         (1792)
#             1824 POP_JUMP_IF_FALSE    (to 1856)
# 
# 13397:        1826 LOAD_FAST            (btn)
#             1828 LOAD_METHOD          (show)
#             1830 CALL_METHOD          (0 positional arguments)
#             1832 POP_TOP
# 
# 13398:        1834 LOAD_FAST            (self)
#             1836 LOAD_METHOD          (set_button_color)
#             1838 LOAD_FAST            (btn)
#             1840 LOAD_FAST            (effect)
#             1842 LOAD_CONST           ("colors")
#             1844 BINARY_SUBSCR
#             1846 LOAD_FAST            (i)
#             1848 BINARY_SUBSCR
#             1850 CALL_METHOD          (2 positional arguments)
#             1852 POP_TOP
#             1854 JUMP_FORWARD         (to 1864)
# 
# 13400:     >> 1856 LOAD_FAST            (btn)
#             1858 LOAD_METHOD          (hide)
#             1860 CALL_METHOD          (0 positional arguments)
#             1862 POP_TOP
#          >> 1864 EXTENDED_ARG         (1792)
#             1866 JUMP_ABSOLUTE        (to 1808)
#          >> 1868 EXTENDED_ARG         (3584)
#             1870 JUMP_FORWARD         (to 5556)
# 
# 13401:     >> 1872 LOAD_FAST            (effect)
#             1874 LOAD_CONST           ("type")
#             1876 BINARY_SUBSCR
#             1878 LOAD_CONST           ("interleave")
#             1880 COMPARE_OP           (==)
#             1882 EXTENDED_ARG         (1792)
#             1884 POP_JUMP_IF_FALSE    (to 1960)
# 
# 13403:        1886 LOAD_GLOBAL          (enumerate)
#             1888 LOAD_FAST            (self)
#             1890 LOAD_ATTR            (color_btns)
#             1892 CALL_FUNCTION        (1 positional argument)
#             1894 GET_ITER
#          >> 1896 FOR_ITER             (to 1956)
#             1898 UNPACK_SEQUENCE      2
#             1900 STORE_FAST           (i)
#             1902 STORE_FAST           (btn)
# 
# 13404:        1904 LOAD_FAST            (i)
#             1906 LOAD_CONST           (4)
#             1908 COMPARE_OP           (<)
#             1910 EXTENDED_ARG         (1792)
#             1912 POP_JUMP_IF_FALSE    (to 1944)
# 
# 13405:        1914 LOAD_FAST            (btn)
#             1916 LOAD_METHOD          (show)
#             1918 CALL_METHOD          (0 positional arguments)
#             1920 POP_TOP
# 
# 13406:        1922 LOAD_FAST            (self)
#             1924 LOAD_METHOD          (set_button_color)
#             1926 LOAD_FAST            (btn)
#             1928 LOAD_FAST            (effect)
#             1930 LOAD_CONST           ("colors")
#             1932 BINARY_SUBSCR
#             1934 LOAD_FAST            (i)
#             1936 BINARY_SUBSCR
#             1938 CALL_METHOD          (2 positional arguments)
#             1940 POP_TOP
#             1942 JUMP_FORWARD         (to 1952)
# 
# 13408:     >> 1944 LOAD_FAST            (btn)
#             1946 LOAD_METHOD          (hide)
#             1948 CALL_METHOD          (0 positional arguments)
#             1950 POP_TOP
#          >> 1952 EXTENDED_ARG         (1792)
#             1954 JUMP_ABSOLUTE        (to 1896)
#          >> 1956 EXTENDED_ARG         (3584)
#             1958 JUMP_FORWARD         (to 5556)
# 
# 13409:     >> 1960 LOAD_FAST            (effect)
#             1962 LOAD_CONST           ("type")
#             1964 BINARY_SUBSCR
#             1966 LOAD_CONST           ("meteor")
#             1968 COMPARE_OP           (==)
#             1970 EXTENDED_ARG         (2048)
#             1972 POP_JUMP_IF_FALSE    (to 2048)
# 
# 13410:        1974 LOAD_GLOBAL          (enumerate)
#             1976 LOAD_FAST            (self)
#             1978 LOAD_ATTR            (color_btns)
#             1980 CALL_FUNCTION        (1 positional argument)
#             1982 GET_ITER
#          >> 1984 FOR_ITER             (to 2044)
#             1986 UNPACK_SEQUENCE      2
#             1988 STORE_FAST           (i)
#             1990 STORE_FAST           (btn)
# 
# 13411:        1992 LOAD_FAST            (i)
#             1994 LOAD_CONST           (3)
#             1996 COMPARE_OP           (<)
#             1998 EXTENDED_ARG         (1792)
#             2000 POP_JUMP_IF_FALSE    (to 2032)
# 
# 13412:        2002 LOAD_FAST            (btn)
#             2004 LOAD_METHOD          (show)
#             2006 CALL_METHOD          (0 positional arguments)
#             2008 POP_TOP
# 
# 13413:        2010 LOAD_FAST            (self)
#             2012 LOAD_METHOD          (set_button_color)
#             2014 LOAD_FAST            (btn)
#             2016 LOAD_FAST            (effect)
#             2018 LOAD_CONST           ("colors")
#             2020 BINARY_SUBSCR
#             2022 LOAD_FAST            (i)
#             2024 BINARY_SUBSCR
#             2026 CALL_METHOD          (2 positional arguments)
#             2028 POP_TOP
#             2030 JUMP_FORWARD         (to 2040)
# 
# 13415:     >> 2032 LOAD_FAST            (btn)
#             2034 LOAD_METHOD          (hide)
#             2036 CALL_METHOD          (0 positional arguments)
#             2038 POP_TOP
#          >> 2040 EXTENDED_ARG         (1792)
#             2042 JUMP_ABSOLUTE        (to 1984)
#          >> 2044 EXTENDED_ARG         (3328)
#             2046 JUMP_FORWARD         (to 5556)
# 
# 13416:     >> 2048 LOAD_FAST            (effect)
#             2050 LOAD_CONST           ("type")
#             2052 BINARY_SUBSCR
#             2054 LOAD_CONST           ("sky_cloud")
#             2056 COMPARE_OP           (==)
#             2058 EXTENDED_ARG         (2048)
#             2060 POP_JUMP_IF_FALSE    (to 2136)
# 
# 13417:        2062 LOAD_GLOBAL          (enumerate)
#             2064 LOAD_FAST            (self)
#             2066 LOAD_ATTR            (color_btns)
#             2068 CALL_FUNCTION        (1 positional argument)
#             2070 GET_ITER
#          >> 2072 FOR_ITER             (to 2132)
#             2074 UNPACK_SEQUENCE      2
#             2076 STORE_FAST           (i)
#             2078 STORE_FAST           (btn)
# 
# 13418:        2080 LOAD_FAST            (i)
#             2082 LOAD_CONST           (2)
#             2084 COMPARE_OP           (<)
#             2086 EXTENDED_ARG         (2048)
#             2088 POP_JUMP_IF_FALSE    (to 2120)
# 
# 13419:        2090 LOAD_FAST            (btn)
#             2092 LOAD_METHOD          (show)
#             2094 CALL_METHOD          (0 positional arguments)
#             2096 POP_TOP
# 
# 13420:        2098 LOAD_FAST            (self)
#             2100 LOAD_METHOD          (set_button_color)
#             2102 LOAD_FAST            (btn)
#             2104 LOAD_FAST            (effect)
#             2106 LOAD_CONST           ("colors")
#             2108 BINARY_SUBSCR
#             2110 LOAD_FAST            (i)
#             2112 BINARY_SUBSCR
#             2114 CALL_METHOD          (2 positional arguments)
#             2116 POP_TOP
#             2118 JUMP_FORWARD         (to 2128)
# 
# 13422:     >> 2120 LOAD_FAST            (btn)
#             2122 LOAD_METHOD          (hide)
#             2124 CALL_METHOD          (0 positional arguments)
#             2126 POP_TOP
#          >> 2128 EXTENDED_ARG         (2048)
#             2130 JUMP_ABSOLUTE        (to 2072)
#          >> 2132 EXTENDED_ARG         (3328)
#             2134 JUMP_FORWARD         (to 5556)
# 
# 13423:     >> 2136 LOAD_FAST            (effect)
#             2138 LOAD_CONST           ("type")
#             2140 BINARY_SUBSCR
#             2142 LOAD_CONST           ("camo_fade")
#             2144 COMPARE_OP           (==)
#             2146 EXTENDED_ARG         (2048)
#             2148 POP_JUMP_IF_FALSE    (to 2224)
# 
# 13424:        2150 LOAD_GLOBAL          (enumerate)
#             2152 LOAD_FAST            (self)
#             2154 LOAD_ATTR            (color_btns)
#             2156 CALL_FUNCTION        (1 positional argument)
#             2158 GET_ITER
#          >> 2160 FOR_ITER             (to 2220)
#             2162 UNPACK_SEQUENCE      2
#             2164 STORE_FAST           (i)
#             2166 STORE_FAST           (btn)
# 
# 13425:        2168 LOAD_FAST            (i)
#             2170 LOAD_CONST           (4)
#             2172 COMPARE_OP           (<)
#             2174 EXTENDED_ARG         (2048)
#             2176 POP_JUMP_IF_FALSE    (to 2208)
# 
# 13426:        2178 LOAD_FAST            (btn)
#             2180 LOAD_METHOD          (show)
#             2182 CALL_METHOD          (0 positional arguments)
#             2184 POP_TOP
# 
# 13427:        2186 LOAD_FAST            (self)
#             2188 LOAD_METHOD          (set_button_color)
#             2190 LOAD_FAST            (btn)
#             2192 LOAD_FAST            (effect)
#             2194 LOAD_CONST           ("colors")
#             2196 BINARY_SUBSCR
#             2198 LOAD_FAST            (i)
#             2200 BINARY_SUBSCR
#             2202 CALL_METHOD          (2 positional arguments)
#             2204 POP_TOP
#             2206 JUMP_FORWARD         (to 2216)
# 
# 13429:     >> 2208 LOAD_FAST            (btn)
#             2210 LOAD_METHOD          (hide)
#             2212 CALL_METHOD          (0 positional arguments)
#             2214 POP_TOP
#          >> 2216 EXTENDED_ARG         (2048)
#             2218 JUMP_ABSOLUTE        (to 2160)
#          >> 2220 EXTENDED_ARG         (3328)
#             2222 JUMP_FORWARD         (to 5556)
# 
# 13430:     >> 2224 LOAD_FAST            (effect)
#             2226 LOAD_CONST           ("type")
#             2228 BINARY_SUBSCR
#             2230 LOAD_CONST           ("lasers_lasers")
#             2232 COMPARE_OP           (==)
#             2234 EXTENDED_ARG         (2304)
#             2236 POP_JUMP_IF_FALSE    (to 2312)
# 
# 13431:        2238 LOAD_GLOBAL          (enumerate)
#             2240 LOAD_FAST            (self)
#             2242 LOAD_ATTR            (color_btns)
#             2244 CALL_FUNCTION        (1 positional argument)
#             2246 GET_ITER
#          >> 2248 FOR_ITER             (to 2308)
#             2250 UNPACK_SEQUENCE      2
#             2252 STORE_FAST           (i)
#             2254 STORE_FAST           (btn)
# 
# 13432:        2256 LOAD_FAST            (i)
#             2258 LOAD_CONST           (2)
#             2260 COMPARE_OP           (<)
#             2262 EXTENDED_ARG         (2048)
#             2264 POP_JUMP_IF_FALSE    (to 2296)
# 
# 13433:        2266 LOAD_FAST            (btn)
#             2268 LOAD_METHOD          (show)
#             2270 CALL_METHOD          (0 positional arguments)
#             2272 POP_TOP
# 
# 13434:        2274 LOAD_FAST            (self)
#             2276 LOAD_METHOD          (set_button_color)
#             2278 LOAD_FAST            (btn)
#             2280 LOAD_FAST            (effect)
#             2282 LOAD_CONST           ("colors")
#             2284 BINARY_SUBSCR
#             2286 LOAD_FAST            (i)
#             2288 BINARY_SUBSCR
#             2290 CALL_METHOD          (2 positional arguments)
#             2292 POP_TOP
#             2294 JUMP_FORWARD         (to 2304)
# 
# 13436:     >> 2296 LOAD_FAST            (btn)
#             2298 LOAD_METHOD          (hide)
#             2300 CALL_METHOD          (0 positional arguments)
#             2302 POP_TOP
#          >> 2304 EXTENDED_ARG         (2048)
#             2306 JUMP_ABSOLUTE        (to 2248)
#          >> 2308 EXTENDED_ARG         (3072)
#             2310 JUMP_FORWARD         (to 5556)
# 
# 13437:     >> 2312 LOAD_FAST            (effect)
#             2314 LOAD_CONST           ("type")
#             2316 BINARY_SUBSCR
#             2318 LOAD_CONST           ("storm")
#             2320 COMPARE_OP           (==)
#             2322 EXTENDED_ARG         (2304)
#             2324 POP_JUMP_IF_FALSE    (to 2400)
# 
# 13438:        2326 LOAD_GLOBAL          (enumerate)
#             2328 LOAD_FAST            (self)
#             2330 LOAD_ATTR            (color_btns)
#             2332 CALL_FUNCTION        (1 positional argument)
#             2334 GET_ITER
#          >> 2336 FOR_ITER             (to 2396)
#             2338 UNPACK_SEQUENCE      2
#             2340 STORE_FAST           (i)
#             2342 STORE_FAST           (btn)
# 
# 13439:        2344 LOAD_FAST            (i)
#             2346 LOAD_CONST           (3)
#             2348 COMPARE_OP           (<)
#             2350 EXTENDED_ARG         (2304)
#             2352 POP_JUMP_IF_FALSE    (to 2384)
# 
# 13440:        2354 LOAD_FAST            (btn)
#             2356 LOAD_METHOD          (show)
#             2358 CALL_METHOD          (0 positional arguments)
#             2360 POP_TOP
# 
# 13441:        2362 LOAD_FAST            (self)
#             2364 LOAD_METHOD          (set_button_color)
#             2366 LOAD_FAST            (btn)
#             2368 LOAD_FAST            (effect)
#             2370 LOAD_CONST           ("colors")
#             2372 BINARY_SUBSCR
#             2374 LOAD_FAST            (i)
#             2376 BINARY_SUBSCR
#             2378 CALL_METHOD          (2 positional arguments)
#             2380 POP_TOP
#             2382 JUMP_FORWARD         (to 2392)
# 
# 13443:     >> 2384 LOAD_FAST            (btn)
#             2386 LOAD_METHOD          (hide)
#             2388 CALL_METHOD          (0 positional arguments)
#             2390 POP_TOP
#          >> 2392 EXTENDED_ARG         (2304)
#             2394 JUMP_ABSOLUTE        (to 2336)
#          >> 2396 EXTENDED_ARG         (3072)
#             2398 JUMP_FORWARD         (to 5556)
# 
# 13444:     >> 2400 LOAD_FAST            (effect)
#             2402 LOAD_CONST           ("type")
#             2404 BINARY_SUBSCR
#             2406 LOAD_CONST           ("explode")
#             2408 COMPARE_OP           (==)
#             2410 EXTENDED_ARG         (2304)
#             2412 POP_JUMP_IF_FALSE    (to 2488)
# 
# 13446:        2414 LOAD_GLOBAL          (enumerate)
#             2416 LOAD_FAST            (self)
#             2418 LOAD_ATTR            (color_btns)
#             2420 CALL_FUNCTION        (1 positional argument)
#             2422 GET_ITER
#          >> 2424 FOR_ITER             (to 2484)
#             2426 UNPACK_SEQUENCE      2
#             2428 STORE_FAST           (i)
#             2430 STORE_FAST           (btn)
# 
# 13447:        2432 LOAD_FAST            (i)
#             2434 LOAD_CONST           (2)
#             2436 COMPARE_OP           (<)
#             2438 EXTENDED_ARG         (2304)
#             2440 POP_JUMP_IF_FALSE    (to 2472)
# 
# 13448:        2442 LOAD_FAST            (btn)
#             2444 LOAD_METHOD          (show)
#             2446 CALL_METHOD          (0 positional arguments)
#             2448 POP_TOP
# 
# 13449:        2450 LOAD_FAST            (self)
#             2452 LOAD_METHOD          (set_button_color)
#             2454 LOAD_FAST            (btn)
#             2456 LOAD_FAST            (effect)
#             2458 LOAD_CONST           ("colors")
#             2460 BINARY_SUBSCR
#             2462 LOAD_FAST            (i)
#             2464 BINARY_SUBSCR
#             2466 CALL_METHOD          (2 positional arguments)
#             2468 POP_TOP
#             2470 JUMP_FORWARD         (to 2480)
# 
# 13451:     >> 2472 LOAD_FAST            (btn)
#             2474 LOAD_METHOD          (hide)
#             2476 CALL_METHOD          (0 positional arguments)
#             2478 POP_TOP
#          >> 2480 EXTENDED_ARG         (2304)
#             2482 JUMP_ABSOLUTE        (to 2424)
#          >> 2484 EXTENDED_ARG         (2816)
#             2486 JUMP_FORWARD         (to 5556)
# 
# 13452:     >> 2488 LOAD_FAST            (effect)
#             2490 LOAD_CONST           ("type")
#             2492 BINARY_SUBSCR
#             2494 LOAD_CONST           ("saiyan")
#             2496 COMPARE_OP           (==)
#             2498 EXTENDED_ARG         (2560)
#             2500 POP_JUMP_IF_FALSE    (to 2576)
# 
# 13453:        2502 LOAD_GLOBAL          (enumerate)
#             2504 LOAD_FAST            (self)
#             2506 LOAD_ATTR            (color_btns)
#             2508 CALL_FUNCTION        (1 positional argument)
#             2510 GET_ITER
#          >> 2512 FOR_ITER             (to 2572)
#             2514 UNPACK_SEQUENCE      2
#             2516 STORE_FAST           (i)
#             2518 STORE_FAST           (btn)
# 
# 13454:        2520 LOAD_FAST            (i)
#             2522 LOAD_CONST           (3)
#             2524 COMPARE_OP           (<)
#             2526 EXTENDED_ARG         (2560)
#             2528 POP_JUMP_IF_FALSE    (to 2560)
# 
# 13455:        2530 LOAD_FAST            (btn)
#             2532 LOAD_METHOD          (show)
#             2534 CALL_METHOD          (0 positional arguments)
#             2536 POP_TOP
# 
# 13456:        2538 LOAD_FAST            (self)
#             2540 LOAD_METHOD          (set_button_color)
#             2542 LOAD_FAST            (btn)
#             2544 LOAD_FAST            (effect)
#             2546 LOAD_CONST           ("colors")
#             2548 BINARY_SUBSCR
#             2550 LOAD_FAST            (i)
#             2552 BINARY_SUBSCR
#             2554 CALL_METHOD          (2 positional arguments)
#             2556 POP_TOP
#             2558 JUMP_FORWARD         (to 2568)
# 
# 13458:     >> 2560 LOAD_FAST            (btn)
#             2562 LOAD_METHOD          (hide)
#             2564 CALL_METHOD          (0 positional arguments)
#             2566 POP_TOP
#          >> 2568 EXTENDED_ARG         (2304)
#             2570 JUMP_ABSOLUTE        (to 2512)
#          >> 2572 EXTENDED_ARG         (2816)
#             2574 JUMP_FORWARD         (to 5556)
# 
# 13459:     >> 2576 LOAD_FAST            (effect)
#             2578 LOAD_CONST           ("type")
#             2580 BINARY_SUBSCR
#             2582 LOAD_CONST           ("spiral")
#             2584 COMPARE_OP           (==)
#             2586 EXTENDED_ARG         (2560)
#             2588 POP_JUMP_IF_FALSE    (to 2664)
# 
# 13460:        2590 LOAD_GLOBAL          (enumerate)
#             2592 LOAD_FAST            (self)
#             2594 LOAD_ATTR            (color_btns)
#             2596 CALL_FUNCTION        (1 positional argument)
#             2598 GET_ITER
#          >> 2600 FOR_ITER             (to 2660)
#             2602 UNPACK_SEQUENCE      2
#             2604 STORE_FAST           (i)
#             2606 STORE_FAST           (btn)
# 
# 13461:        2608 LOAD_FAST            (i)
#             2610 LOAD_CONST           (4)
#             2612 COMPARE_OP           (<)
#             2614 EXTENDED_ARG         (2560)
#             2616 POP_JUMP_IF_FALSE    (to 2648)
# 
# 13462:        2618 LOAD_FAST            (btn)
#             2620 LOAD_METHOD          (show)
#             2622 CALL_METHOD          (0 positional arguments)
#             2624 POP_TOP
# 
# 13463:        2626 LOAD_FAST            (self)
#             2628 LOAD_METHOD          (set_button_color)
#             2630 LOAD_FAST            (btn)
#             2632 LOAD_FAST            (effect)
#             2634 LOAD_CONST           ("colors")
#             2636 BINARY_SUBSCR
#             2638 LOAD_FAST            (i)
#             2640 BINARY_SUBSCR
#             2642 CALL_METHOD          (2 positional arguments)
#             2644 POP_TOP
#             2646 JUMP_FORWARD         (to 2656)
# 
# 13465:     >> 2648 LOAD_FAST            (btn)
#             2650 LOAD_METHOD          (hide)
#             2652 CALL_METHOD          (0 positional arguments)
#             2654 POP_TOP
#          >> 2656 EXTENDED_ARG         (2560)
#             2658 JUMP_ABSOLUTE        (to 2600)
#          >> 2660 EXTENDED_ARG         (2816)
#             2662 JUMP_FORWARD         (to 5556)
# 
# 13466:     >> 2664 LOAD_FAST            (effect)
#             2666 LOAD_CONST           ("type")
#             2668 BINARY_SUBSCR
#             2670 LOAD_CONST           ("poison")
#             2672 COMPARE_OP           (==)
#             2674 EXTENDED_ARG         (2560)
#             2676 POP_JUMP_IF_FALSE    (to 2752)
# 
# 13467:        2678 LOAD_GLOBAL          (enumerate)
#             2680 LOAD_FAST            (self)
#             2682 LOAD_ATTR            (color_btns)
#             2684 CALL_FUNCTION        (1 positional argument)
#             2686 GET_ITER
#          >> 2688 FOR_ITER             (to 2748)
#             2690 UNPACK_SEQUENCE      2
#             2692 STORE_FAST           (i)
#             2694 STORE_FAST           (btn)
# 
# 13468:        2696 LOAD_FAST            (i)
#             2698 LOAD_CONST           (4)
#             2700 COMPARE_OP           (<)
#             2702 EXTENDED_ARG         (2560)
#             2704 POP_JUMP_IF_FALSE    (to 2736)
# 
# 13469:        2706 LOAD_FAST            (btn)
#             2708 LOAD_METHOD          (show)
#             2710 CALL_METHOD          (0 positional arguments)
#             2712 POP_TOP
# 
# 13470:        2714 LOAD_FAST            (self)
#             2716 LOAD_METHOD          (set_button_color)
#             2718 LOAD_FAST            (btn)
#             2720 LOAD_FAST            (effect)
#             2722 LOAD_CONST           ("colors")
#             2724 BINARY_SUBSCR
#             2726 LOAD_FAST            (i)
#             2728 BINARY_SUBSCR
#             2730 CALL_METHOD          (2 positional arguments)
#             2732 POP_TOP
#             2734 JUMP_FORWARD         (to 2744)
# 
# 13472:     >> 2736 LOAD_FAST            (btn)
#             2738 LOAD_METHOD          (hide)
#             2740 CALL_METHOD          (0 positional arguments)
#             2742 POP_TOP
#          >> 2744 EXTENDED_ARG         (2560)
#             2746 JUMP_ABSOLUTE        (to 2688)
#          >> 2748 EXTENDED_ARG         (2560)
#             2750 JUMP_FORWARD         (to 5556)
# 
# 13473:     >> 2752 LOAD_FAST            (effect)
#             2754 LOAD_CONST           ("type")
#             2756 BINARY_SUBSCR
#             2758 LOAD_CONST           ("windmill")
#             2760 COMPARE_OP           (==)
#             2762 EXTENDED_ARG         (2816)
#             2764 POP_JUMP_IF_FALSE    (to 2840)
# 
# 13474:        2766 LOAD_GLOBAL          (enumerate)
#             2768 LOAD_FAST            (self)
#             2770 LOAD_ATTR            (color_btns)
#             2772 CALL_FUNCTION        (1 positional argument)
#             2774 GET_ITER
#          >> 2776 FOR_ITER             (to 2836)
#             2778 UNPACK_SEQUENCE      2
#             2780 STORE_FAST           (i)
#             2782 STORE_FAST           (btn)
# 
# 13475:        2784 LOAD_FAST            (i)
#             2786 LOAD_CONST           (2)
#             2788 COMPARE_OP           (<)
#             2790 EXTENDED_ARG         (2816)
#             2792 POP_JUMP_IF_FALSE    (to 2824)
# 
# 13476:        2794 LOAD_FAST            (btn)
#             2796 LOAD_METHOD          (show)
#             2798 CALL_METHOD          (0 positional arguments)
#             2800 POP_TOP
# 
# 13477:        2802 LOAD_FAST            (self)
#             2804 LOAD_METHOD          (set_button_color)
#             2806 LOAD_FAST            (btn)
#             2808 LOAD_FAST            (effect)
#             2810 LOAD_CONST           ("colors")
#             2812 BINARY_SUBSCR
#             2814 LOAD_FAST            (i)
#             2816 BINARY_SUBSCR
#             2818 CALL_METHOD          (2 positional arguments)
#             2820 POP_TOP
#             2822 JUMP_FORWARD         (to 2832)
# 
# 13479:     >> 2824 LOAD_FAST            (btn)
#             2826 LOAD_METHOD          (hide)
#             2828 CALL_METHOD          (0 positional arguments)
#             2830 POP_TOP
#          >> 2832 EXTENDED_ARG         (2560)
#             2834 JUMP_ABSOLUTE        (to 2776)
#          >> 2836 EXTENDED_ARG         (2560)
#             2838 JUMP_FORWARD         (to 5556)
# 
# 13480:     >> 2840 LOAD_FAST            (effect)
#             2842 LOAD_CONST           ("type")
#             2844 BINARY_SUBSCR
#             2846 LOAD_CONST           ("maple_leaf")
#             2848 COMPARE_OP           (==)
#             2850 EXTENDED_ARG         (2816)
#             2852 POP_JUMP_IF_FALSE    (to 2928)
# 
# 13481:        2854 LOAD_GLOBAL          (enumerate)
#             2856 LOAD_FAST            (self)
#             2858 LOAD_ATTR            (color_btns)
#             2860 CALL_FUNCTION        (1 positional argument)
#             2862 GET_ITER
#          >> 2864 FOR_ITER             (to 2924)
#             2866 UNPACK_SEQUENCE      2
#             2868 STORE_FAST           (i)
#             2870 STORE_FAST           (btn)
# 
# 13482:        2872 LOAD_FAST            (i)
#             2874 LOAD_CONST           (4)
#             2876 COMPARE_OP           (<)
#             2878 EXTENDED_ARG         (2816)
#             2880 POP_JUMP_IF_FALSE    (to 2912)
# 
# 13483:        2882 LOAD_FAST            (btn)
#             2884 LOAD_METHOD          (show)
#             2886 CALL_METHOD          (0 positional arguments)
#             2888 POP_TOP
# 
# 13484:        2890 LOAD_FAST            (self)
#             2892 LOAD_METHOD          (set_button_color)
#             2894 LOAD_FAST            (btn)
#             2896 LOAD_FAST            (effect)
#             2898 LOAD_CONST           ("colors")
#             2900 BINARY_SUBSCR
#             2902 LOAD_FAST            (i)
#             2904 BINARY_SUBSCR
#             2906 CALL_METHOD          (2 positional arguments)
#             2908 POP_TOP
#             2910 JUMP_FORWARD         (to 2920)
# 
# 13486:     >> 2912 LOAD_FAST            (btn)
#             2914 LOAD_METHOD          (hide)
#             2916 CALL_METHOD          (0 positional arguments)
#             2918 POP_TOP
#          >> 2920 EXTENDED_ARG         (2816)
#             2922 JUMP_ABSOLUTE        (to 2864)
#          >> 2924 EXTENDED_ARG         (2560)
#             2926 JUMP_FORWARD         (to 5556)
# 
# 13487:     >> 2928 LOAD_FAST            (effect)
#             2930 LOAD_CONST           ("type")
#             2932 BINARY_SUBSCR
#             2934 LOAD_CONST           ("lightsaber_battle")
#             2936 COMPARE_OP           (==)
#             2938 EXTENDED_ARG         (2816)
#             2940 POP_JUMP_IF_FALSE    (to 3026)
# 
# 13488:        2942 LOAD_FAST            (self)
#             2944 LOAD_ATTR            (speed_slider)
#             2946 LOAD_METHOD          (show)
#             2948 CALL_METHOD          (0 positional arguments)
#             2950 POP_TOP
# 
# 13489:        2952 LOAD_GLOBAL          (enumerate)
#             2954 LOAD_FAST            (self)
#             2956 LOAD_ATTR            (color_btns)
#             2958 CALL_FUNCTION        (1 positional argument)
#             2960 GET_ITER
#          >> 2962 FOR_ITER             (to 3022)
#             2964 UNPACK_SEQUENCE      2
#             2966 STORE_FAST           (i)
#             2968 STORE_FAST           (btn)
# 
# 13490:        2970 LOAD_FAST            (i)
#             2972 LOAD_CONST           (2)
#             2974 COMPARE_OP           (<)
#             2976 EXTENDED_ARG         (2816)
#             2978 POP_JUMP_IF_FALSE    (to 3010)
# 
# 13491:        2980 LOAD_FAST            (btn)
#             2982 LOAD_METHOD          (show)
#             2984 CALL_METHOD          (0 positional arguments)
#             2986 POP_TOP
# 
# 13492:        2988 LOAD_FAST            (self)
#             2990 LOAD_METHOD          (set_button_color)
#             2992 LOAD_FAST            (btn)
#             2994 LOAD_FAST            (effect)
#             2996 LOAD_CONST           ("colors")
#             2998 BINARY_SUBSCR
#             3000 LOAD_FAST            (i)
#             3002 BINARY_SUBSCR
#             3004 CALL_METHOD          (2 positional arguments)
#             3006 POP_TOP
#             3008 JUMP_FORWARD         (to 3018)
# 
# 13494:     >> 3010 LOAD_FAST            (btn)
#             3012 LOAD_METHOD          (hide)
#             3014 CALL_METHOD          (0 positional arguments)
#             3016 POP_TOP
#          >> 3018 EXTENDED_ARG         (2816)
#             3020 JUMP_ABSOLUTE        (to 2962)
#          >> 3022 EXTENDED_ARG         (2304)
#             3024 JUMP_FORWARD         (to 5556)
# 
# 13495:     >> 3026 LOAD_FAST            (effect)
#             3028 LOAD_CONST           ("type")
#             3030 BINARY_SUBSCR
#             3032 LOAD_CONST           ("police_light")
#             3034 COMPARE_OP           (==)
#             3036 EXTENDED_ARG         (3072)
#             3038 POP_JUMP_IF_FALSE    (to 3124)
# 
# 13496:        3040 LOAD_FAST            (self)
#             3042 LOAD_ATTR            (speed_slider)
#             3044 LOAD_METHOD          (show)
#             3046 CALL_METHOD          (0 positional arguments)
#             3048 POP_TOP
# 
# 13497:        3050 LOAD_GLOBAL          (enumerate)
#             3052 LOAD_FAST            (self)
#             3054 LOAD_ATTR            (color_btns)
#             3056 CALL_FUNCTION        (1 positional argument)
#             3058 GET_ITER
#          >> 3060 FOR_ITER             (to 3120)
#             3062 UNPACK_SEQUENCE      2
#             3064 STORE_FAST           (i)
#             3066 STORE_FAST           (btn)
# 
# 13498:        3068 LOAD_FAST            (i)
#             3070 LOAD_CONST           (3)
#             3072 COMPARE_OP           (<)
#             3074 EXTENDED_ARG         (3072)
#             3076 POP_JUMP_IF_FALSE    (to 3108)
# 
# 13499:        3078 LOAD_FAST            (btn)
#             3080 LOAD_METHOD          (show)
#             3082 CALL_METHOD          (0 positional arguments)
#             3084 POP_TOP
# 
# 13500:        3086 LOAD_FAST            (self)
#             3088 LOAD_METHOD          (set_button_color)
#             3090 LOAD_FAST            (btn)
#             3092 LOAD_FAST            (effect)
#             3094 LOAD_CONST           ("colors")
#             3096 BINARY_SUBSCR
#             3098 LOAD_FAST            (i)
#             3100 BINARY_SUBSCR
#             3102 CALL_METHOD          (2 positional arguments)
#             3104 POP_TOP
#             3106 JUMP_FORWARD         (to 3116)
# 
# 13502:     >> 3108 LOAD_FAST            (btn)
#             3110 LOAD_METHOD          (hide)
#             3112 CALL_METHOD          (0 positional arguments)
#             3114 POP_TOP
#          >> 3116 EXTENDED_ARG         (2816)
#             3118 JUMP_ABSOLUTE        (to 3060)
#          >> 3120 EXTENDED_ARG         (2304)
#             3122 JUMP_FORWARD         (to 5556)
# 
# 13503:     >> 3124 LOAD_FAST            (effect)
#             3126 LOAD_CONST           ("type")
#             3128 BINARY_SUBSCR
#             3130 LOAD_CONST           ("sphere_3d")
#             3132 COMPARE_OP           (==)
#             3134 EXTENDED_ARG         (3072)
#             3136 POP_JUMP_IF_FALSE    (to 3222)
# 
# 13504:        3138 LOAD_FAST            (self)
#             3140 LOAD_ATTR            (speed_slider)
#             3142 LOAD_METHOD          (show)
#             3144 CALL_METHOD          (0 positional arguments)
#             3146 POP_TOP
# 
# 13506:        3148 LOAD_GLOBAL          (enumerate)
#             3150 LOAD_FAST            (self)
#             3152 LOAD_ATTR            (color_btns)
#             3154 CALL_FUNCTION        (1 positional argument)
#             3156 GET_ITER
#          >> 3158 FOR_ITER             (to 3218)
#             3160 UNPACK_SEQUENCE      2
#             3162 STORE_FAST           (i)
#             3164 STORE_FAST           (btn)
# 
# 13507:        3166 LOAD_FAST            (i)
#             3168 LOAD_CONST           (4)
#             3170 COMPARE_OP           (<)
#             3172 EXTENDED_ARG         (3072)
#             3174 POP_JUMP_IF_FALSE    (to 3206)
# 
# 13508:        3176 LOAD_FAST            (btn)
#             3178 LOAD_METHOD          (show)
#             3180 CALL_METHOD          (0 positional arguments)
#             3182 POP_TOP
# 
# 13509:        3184 LOAD_FAST            (self)
#             3186 LOAD_METHOD          (set_button_color)
#             3188 LOAD_FAST            (btn)
#             3190 LOAD_FAST            (effect)
#             3192 LOAD_CONST           ("colors")
#             3194 BINARY_SUBSCR
#             3196 LOAD_FAST            (i)
#             3198 BINARY_SUBSCR
#             3200 CALL_METHOD          (2 positional arguments)
#             3202 POP_TOP
#             3204 JUMP_FORWARD         (to 3214)
# 
# 13511:     >> 3206 LOAD_FAST            (btn)
#             3208 LOAD_METHOD          (hide)
#             3210 CALL_METHOD          (0 positional arguments)
#             3212 POP_TOP
#          >> 3214 EXTENDED_ARG         (3072)
#             3216 JUMP_ABSOLUTE        (to 3158)
#          >> 3218 EXTENDED_ARG         (2304)
#             3220 JUMP_FORWARD         (to 5556)
# 
# 13512:     >> 3222 LOAD_FAST            (effect)
#             3224 LOAD_CONST           ("type")
#             3226 BINARY_SUBSCR
#             3228 LOAD_CONST           ("dream_dream")
#             3230 COMPARE_OP           (==)
#             3232 EXTENDED_ARG         (3072)
#             3234 POP_JUMP_IF_FALSE    (to 3320)
# 
# 13513:        3236 LOAD_FAST            (self)
#             3238 LOAD_ATTR            (speed_slider)
#             3240 LOAD_METHOD          (show)
#             3242 CALL_METHOD          (0 positional arguments)
#             3244 POP_TOP
# 
# 13514:        3246 LOAD_GLOBAL          (enumerate)
#             3248 LOAD_FAST            (self)
#             3250 LOAD_ATTR            (color_btns)
#             3252 CALL_FUNCTION        (1 positional argument)
#             3254 GET_ITER
#          >> 3256 FOR_ITER             (to 3316)
#             3258 UNPACK_SEQUENCE      2
#             3260 STORE_FAST           (i)
#             3262 STORE_FAST           (btn)
# 
# 13515:        3264 LOAD_FAST            (i)
#             3266 LOAD_CONST           (4)
#             3268 COMPARE_OP           (<)
#             3270 EXTENDED_ARG         (3072)
#             3272 POP_JUMP_IF_FALSE    (to 3304)
# 
# 13516:        3274 LOAD_FAST            (btn)
#             3276 LOAD_METHOD          (show)
#             3278 CALL_METHOD          (0 positional arguments)
#             3280 POP_TOP
# 
# 13517:        3282 LOAD_FAST            (self)
#             3284 LOAD_METHOD          (set_button_color)
#             3286 LOAD_FAST            (btn)
#             3288 LOAD_FAST            (effect)
#             3290 LOAD_CONST           ("colors")
#             3292 BINARY_SUBSCR
#             3294 LOAD_FAST            (i)
#             3296 BINARY_SUBSCR
#             3298 CALL_METHOD          (2 positional arguments)
#             3300 POP_TOP
#             3302 JUMP_FORWARD         (to 3312)
# 
# 13519:     >> 3304 LOAD_FAST            (btn)
#             3306 LOAD_METHOD          (hide)
#             3308 CALL_METHOD          (0 positional arguments)
#             3310 POP_TOP
#          >> 3312 EXTENDED_ARG         (3072)
#             3314 JUMP_ABSOLUTE        (to 3256)
#          >> 3316 EXTENDED_ARG         (2048)
#             3318 JUMP_FORWARD         (to 5556)
# 
# 13520:     >> 3320 LOAD_FAST            (effect)
#             3322 LOAD_CONST           ("type")
#             3324 BINARY_SUBSCR
#             3326 LOAD_CONST           ("neon_wave")
#             3328 COMPARE_OP           (==)
#             3330 EXTENDED_ARG         (3328)
#             3332 POP_JUMP_IF_FALSE    (to 3418)
# 
# 13521:        3334 LOAD_FAST            (self)
#             3336 LOAD_ATTR            (speed_slider)
#             3338 LOAD_METHOD          (show)
#             3340 CALL_METHOD          (0 positional arguments)
#             3342 POP_TOP
# 
# 13522:        3344 LOAD_GLOBAL          (enumerate)
#             3346 LOAD_FAST            (self)
#             3348 LOAD_ATTR            (color_btns)
#             3350 CALL_FUNCTION        (1 positional argument)
#             3352 GET_ITER
#          >> 3354 FOR_ITER             (to 3414)
#             3356 UNPACK_SEQUENCE      2
#             3358 STORE_FAST           (i)
#             3360 STORE_FAST           (btn)
# 
# 13523:        3362 LOAD_FAST            (i)
#             3364 LOAD_CONST           (4)
#             3366 COMPARE_OP           (<)
#             3368 EXTENDED_ARG         (3328)
#             3370 POP_JUMP_IF_FALSE    (to 3402)
# 
# 13524:        3372 LOAD_FAST            (btn)
#             3374 LOAD_METHOD          (show)
#             3376 CALL_METHOD          (0 positional arguments)
#             3378 POP_TOP
# 
# 13525:        3380 LOAD_FAST            (self)
#             3382 LOAD_METHOD          (set_button_color)
#             3384 LOAD_FAST            (btn)
#             3386 LOAD_FAST            (effect)
#             3388 LOAD_CONST           ("colors")
#             3390 BINARY_SUBSCR
#             3392 LOAD_FAST            (i)
#             3394 BINARY_SUBSCR
#             3396 CALL_METHOD          (2 positional arguments)
#             3398 POP_TOP
#             3400 JUMP_FORWARD         (to 3410)
# 
# 13527:     >> 3402 LOAD_FAST            (btn)
#             3404 LOAD_METHOD          (hide)
#             3406 CALL_METHOD          (0 positional arguments)
#             3408 POP_TOP
#          >> 3410 EXTENDED_ARG         (3328)
#             3412 JUMP_ABSOLUTE        (to 3354)
#          >> 3414 EXTENDED_ARG         (2048)
#             3416 JUMP_FORWARD         (to 5556)
# 
# 13528:     >> 3418 LOAD_FAST            (effect)
#             3420 LOAD_CONST           ("type")
#             3422 BINARY_SUBSCR
#             3424 LOAD_CONST           ("fireflies")
#             3426 COMPARE_OP           (==)
#             3428 EXTENDED_ARG         (3328)
#             3430 POP_JUMP_IF_FALSE    (to 3516)
# 
# 13529:        3432 LOAD_FAST            (self)
#             3434 LOAD_ATTR            (speed_slider)
#             3436 LOAD_METHOD          (show)
#             3438 CALL_METHOD          (0 positional arguments)
#             3440 POP_TOP
# 
# 13530:        3442 LOAD_GLOBAL          (enumerate)
#             3444 LOAD_FAST            (self)
#             3446 LOAD_ATTR            (color_btns)
#             3448 CALL_FUNCTION        (1 positional argument)
#             3450 GET_ITER
#          >> 3452 FOR_ITER             (to 3512)
#             3454 UNPACK_SEQUENCE      2
#             3456 STORE_FAST           (i)
#             3458 STORE_FAST           (btn)
# 
# 13531:        3460 LOAD_FAST            (i)
#             3462 LOAD_CONST           (4)
#             3464 COMPARE_OP           (<)
#             3466 EXTENDED_ARG         (3328)
#             3468 POP_JUMP_IF_FALSE    (to 3500)
# 
# 13532:        3470 LOAD_FAST            (btn)
#             3472 LOAD_METHOD          (show)
#             3474 CALL_METHOD          (0 positional arguments)
#             3476 POP_TOP
# 
# 13533:        3478 LOAD_FAST            (self)
#             3480 LOAD_METHOD          (set_button_color)
#             3482 LOAD_FAST            (btn)
#             3484 LOAD_FAST            (effect)
#             3486 LOAD_CONST           ("colors")
#             3488 BINARY_SUBSCR
#             3490 LOAD_FAST            (i)
#             3492 BINARY_SUBSCR
#             3494 CALL_METHOD          (2 positional arguments)
#             3496 POP_TOP
#             3498 JUMP_FORWARD         (to 3508)
# 
# 13535:     >> 3500 LOAD_FAST            (btn)
#             3502 LOAD_METHOD          (hide)
#             3504 CALL_METHOD          (0 positional arguments)
#             3506 POP_TOP
#          >> 3508 EXTENDED_ARG         (3328)
#             3510 JUMP_ABSOLUTE        (to 3452)
#          >> 3512 EXTENDED_ARG         (1792)
#             3514 JUMP_FORWARD         (to 5556)
# 
# 13536:     >> 3516 LOAD_FAST            (effect)
#             3518 LOAD_CONST           ("type")
#             3520 BINARY_SUBSCR
#             3522 LOAD_CONST           ("hellfire")
#             3524 COMPARE_OP           (==)
#             3526 EXTENDED_ARG         (3584)
#             3528 POP_JUMP_IF_FALSE    (to 3604)
# 
# 13537:        3530 LOAD_GLOBAL          (enumerate)
#             3532 LOAD_FAST            (self)
#             3534 LOAD_ATTR            (color_btns)
#             3536 CALL_FUNCTION        (1 positional argument)
#             3538 GET_ITER
#          >> 3540 FOR_ITER             (to 3600)
#             3542 UNPACK_SEQUENCE      2
#             3544 STORE_FAST           (i)
#             3546 STORE_FAST           (btn)
# 
# 13538:        3548 LOAD_FAST            (i)
#             3550 LOAD_CONST           (4)
#             3552 COMPARE_OP           (<)
#             3554 EXTENDED_ARG         (3584)
#             3556 POP_JUMP_IF_FALSE    (to 3588)
# 
# 13539:        3558 LOAD_FAST            (btn)
#             3560 LOAD_METHOD          (show)
#             3562 CALL_METHOD          (0 positional arguments)
#             3564 POP_TOP
# 
# 13540:        3566 LOAD_FAST            (self)
#             3568 LOAD_METHOD          (set_button_color)
#             3570 LOAD_FAST            (btn)
#             3572 LOAD_FAST            (effect)
#             3574 LOAD_CONST           ("colors")
#             3576 BINARY_SUBSCR
#             3578 LOAD_FAST            (i)
#             3580 BINARY_SUBSCR
#             3582 CALL_METHOD          (2 positional arguments)
#             3584 POP_TOP
#             3586 JUMP_FORWARD         (to 3596)
# 
# 13542:     >> 3588 LOAD_FAST            (btn)
#             3590 LOAD_METHOD          (hide)
#             3592 CALL_METHOD          (0 positional arguments)
#             3594 POP_TOP
#          >> 3596 EXTENDED_ARG         (3328)
#             3598 JUMP_ABSOLUTE        (to 3540)
#          >> 3600 EXTENDED_ARG         (1792)
#             3602 JUMP_FORWARD         (to 5556)
# 
# 13543:     >> 3604 LOAD_FAST            (effect)
#             3606 LOAD_CONST           ("type")
#             3608 BINARY_SUBSCR
#             3610 LOAD_CONST           ("dragon_flame")
#             3612 COMPARE_OP           (==)
#             3614 EXTENDED_ARG         (3584)
#             3616 POP_JUMP_IF_FALSE    (to 3692)
# 
# 13544:        3618 LOAD_GLOBAL          (enumerate)
#             3620 LOAD_FAST            (self)
#             3622 LOAD_ATTR            (color_btns)
#             3624 CALL_FUNCTION        (1 positional argument)
#             3626 GET_ITER
#          >> 3628 FOR_ITER             (to 3688)
#             3630 UNPACK_SEQUENCE      2
#             3632 STORE_FAST           (i)
#             3634 STORE_FAST           (btn)
# 
# 13545:        3636 LOAD_FAST            (i)
#             3638 LOAD_CONST           (4)
#             3640 COMPARE_OP           (<)
#             3642 EXTENDED_ARG         (3584)
#             3644 POP_JUMP_IF_FALSE    (to 3676)
# 
# 13546:        3646 LOAD_FAST            (btn)
#             3648 LOAD_METHOD          (show)
#             3650 CALL_METHOD          (0 positional arguments)
#             3652 POP_TOP
# 
# 13547:        3654 LOAD_FAST            (self)
#             3656 LOAD_METHOD          (set_button_color)
#             3658 LOAD_FAST            (btn)
#             3660 LOAD_FAST            (effect)
#             3662 LOAD_CONST           ("colors")
#             3664 BINARY_SUBSCR
#             3666 LOAD_FAST            (i)
#             3668 BINARY_SUBSCR
#             3670 CALL_METHOD          (2 positional arguments)
#             3672 POP_TOP
#             3674 JUMP_FORWARD         (to 3684)
# 
# 13549:     >> 3676 LOAD_FAST            (btn)
#             3678 LOAD_METHOD          (hide)
#             3680 CALL_METHOD          (0 positional arguments)
#             3682 POP_TOP
#          >> 3684 EXTENDED_ARG         (3584)
#             3686 JUMP_ABSOLUTE        (to 3628)
#          >> 3688 EXTENDED_ARG         (1792)
#             3690 JUMP_FORWARD         (to 5556)
# 
# 13550:     >> 3692 LOAD_FAST            (effect)
#             3694 LOAD_CONST           ("type")
#             3696 BINARY_SUBSCR
#             3698 LOAD_CONST           ("hydrogen")
#             3700 COMPARE_OP           (==)
#             3702 EXTENDED_ARG         (3584)
#             3704 POP_JUMP_IF_FALSE    (to 3780)
# 
# 13551:        3706 LOAD_GLOBAL          (enumerate)
#             3708 LOAD_FAST            (self)
#             3710 LOAD_ATTR            (color_btns)
#             3712 CALL_FUNCTION        (1 positional argument)
#             3714 GET_ITER
#          >> 3716 FOR_ITER             (to 3776)
#             3718 UNPACK_SEQUENCE      2
#             3720 STORE_FAST           (i)
#             3722 STORE_FAST           (btn)
# 
# 13552:        3724 LOAD_FAST            (i)
#             3726 LOAD_CONST           (4)
#             3728 COMPARE_OP           (<)
#             3730 EXTENDED_ARG         (3584)
#             3732 POP_JUMP_IF_FALSE    (to 3764)
# 
# 13553:        3734 LOAD_FAST            (btn)
#             3736 LOAD_METHOD          (show)
#             3738 CALL_METHOD          (0 positional arguments)
#             3740 POP_TOP
# 
# 13554:        3742 LOAD_FAST            (self)
#             3744 LOAD_METHOD          (set_button_color)
#             3746 LOAD_FAST            (btn)
#             3748 LOAD_FAST            (effect)
#             3750 LOAD_CONST           ("colors")
#             3752 BINARY_SUBSCR
#             3754 LOAD_FAST            (i)
#             3756 BINARY_SUBSCR
#             3758 CALL_METHOD          (2 positional arguments)
#             3760 POP_TOP
#             3762 JUMP_FORWARD         (to 3772)
# 
# 13556:     >> 3764 LOAD_FAST            (btn)
#             3766 LOAD_METHOD          (hide)
#             3768 CALL_METHOD          (0 positional arguments)
#             3770 POP_TOP
#          >> 3772 EXTENDED_ARG         (3584)
#             3774 JUMP_ABSOLUTE        (to 3716)
#          >> 3776 EXTENDED_ARG         (1536)
#             3778 JUMP_FORWARD         (to 5556)
# 
# 13557:     >> 3780 LOAD_FAST            (effect)
#             3782 LOAD_CONST           ("type")
#             3784 BINARY_SUBSCR
#             3786 LOAD_CONST           ("hyperspace")
#             3788 COMPARE_OP           (==)
#             3790 EXTENDED_ARG         (3840)
#             3792 POP_JUMP_IF_FALSE    (to 3868)
# 
# 13559:        3794 LOAD_GLOBAL          (enumerate)
#             3796 LOAD_FAST            (self)
#             3798 LOAD_ATTR            (color_btns)
#             3800 CALL_FUNCTION        (1 positional argument)
#             3802 GET_ITER
#          >> 3804 FOR_ITER             (to 3864)
#             3806 UNPACK_SEQUENCE      2
#             3808 STORE_FAST           (i)
#             3810 STORE_FAST           (btn)
# 
# 13560:        3812 LOAD_FAST            (i)
#             3814 LOAD_CONST           (3)
#             3816 COMPARE_OP           (<)
#             3818 EXTENDED_ARG         (3840)
#             3820 POP_JUMP_IF_FALSE    (to 3852)
# 
# 13561:        3822 LOAD_FAST            (btn)
#             3824 LOAD_METHOD          (show)
#             3826 CALL_METHOD          (0 positional arguments)
#             3828 POP_TOP
# 
# 13562:        3830 LOAD_FAST            (self)
#             3832 LOAD_METHOD          (set_button_color)
#             3834 LOAD_FAST            (btn)
#             3836 LOAD_FAST            (effect)
#             3838 LOAD_CONST           ("colors")
#             3840 BINARY_SUBSCR
#             3842 LOAD_FAST            (i)
#             3844 BINARY_SUBSCR
#             3846 CALL_METHOD          (2 positional arguments)
#             3848 POP_TOP
#             3850 JUMP_FORWARD         (to 3860)
# 
# 13564:     >> 3852 LOAD_FAST            (btn)
#             3854 LOAD_METHOD          (hide)
#             3856 CALL_METHOD          (0 positional arguments)
#             3858 POP_TOP
#          >> 3860 EXTENDED_ARG         (3584)
#             3862 JUMP_ABSOLUTE        (to 3804)
#          >> 3864 EXTENDED_ARG         (1536)
#             3866 JUMP_FORWARD         (to 5556)
# 
# 13566:     >> 3868 LOAD_FAST            (effect)
#             3870 LOAD_CONST           ("type")
#             3872 BINARY_SUBSCR
#             3874 LOAD_CONST           ("jellyfish")
#             3876 COMPARE_OP           (==)
#             3878 EXTENDED_ARG         (3840)
#             3880 POP_JUMP_IF_FALSE    (to 3956)
# 
# 13567:        3882 LOAD_GLOBAL          (enumerate)
#             3884 LOAD_FAST            (self)
#             3886 LOAD_ATTR            (color_btns)
#             3888 CALL_FUNCTION        (1 positional argument)
#             3890 GET_ITER
#          >> 3892 FOR_ITER             (to 3952)
#             3894 UNPACK_SEQUENCE      2
#             3896 STORE_FAST           (i)
#             3898 STORE_FAST           (btn)
# 
# 13568:        3900 LOAD_FAST            (i)
#             3902 LOAD_CONST           (2)
#             3904 COMPARE_OP           (<)
#             3906 EXTENDED_ARG         (3840)
#             3908 POP_JUMP_IF_FALSE    (to 3940)
# 
# 13569:        3910 LOAD_FAST            (btn)
#             3912 LOAD_METHOD          (show)
#             3914 CALL_METHOD          (0 positional arguments)
#             3916 POP_TOP
# 
# 13570:        3918 LOAD_FAST            (self)
#             3920 LOAD_METHOD          (set_button_color)
#             3922 LOAD_FAST            (btn)
#             3924 LOAD_FAST            (effect)
#             3926 LOAD_CONST           ("colors")
#             3928 BINARY_SUBSCR
#             3930 LOAD_FAST            (i)
#             3932 BINARY_SUBSCR
#             3934 CALL_METHOD          (2 positional arguments)
#             3936 POP_TOP
#             3938 JUMP_FORWARD         (to 3948)
# 
# 13572:     >> 3940 LOAD_FAST            (btn)
#             3942 LOAD_METHOD          (hide)
#             3944 CALL_METHOD          (0 positional arguments)
#             3946 POP_TOP
#          >> 3948 EXTENDED_ARG         (3840)
#             3950 JUMP_ABSOLUTE        (to 3892)
#          >> 3952 EXTENDED_ARG         (1536)
#             3954 JUMP_FORWARD         (to 5556)
# 
# 13573:     >> 3956 LOAD_FAST            (effect)
#             3958 LOAD_CONST           ("type")
#             3960 BINARY_SUBSCR
#             3962 LOAD_CONST           ("koi_pond")
#             3964 COMPARE_OP           (==)
#             3966 EXTENDED_ARG         (3840)
#             3968 POP_JUMP_IF_FALSE    (to 4052)
# 
# 13574:        3970 LOAD_GLOBAL          (enumerate)
#             3972 LOAD_FAST            (self)
#             3974 LOAD_ATTR            (color_btns)
#             3976 CALL_FUNCTION        (1 positional argument)
#             3978 GET_ITER
#          >> 3980 FOR_ITER             (to 4048)
#             3982 UNPACK_SEQUENCE      2
#             3984 STORE_FAST           (i)
#             3986 STORE_FAST           (btn)
# 
# 13575:        3988 LOAD_FAST            (i)
#             3990 LOAD_GLOBAL          (len)
#             3992 LOAD_FAST            (effect)
#             3994 LOAD_CONST           ("colors")
#             3996 BINARY_SUBSCR
#             3998 CALL_FUNCTION        (1 positional argument)
#             4000 COMPARE_OP           (<)
#             4002 EXTENDED_ARG         (3840)
#             4004 POP_JUMP_IF_FALSE    (to 4036)
# 
# 13576:        4006 LOAD_FAST            (btn)
#             4008 LOAD_METHOD          (show)
#             4010 CALL_METHOD          (0 positional arguments)
#             4012 POP_TOP
# 
# 13577:        4014 LOAD_FAST            (self)
#             4016 LOAD_METHOD          (set_button_color)
#             4018 LOAD_FAST            (btn)
#             4020 LOAD_FAST            (effect)
#             4022 LOAD_CONST           ("colors")
#             4024 BINARY_SUBSCR
#             4026 LOAD_FAST            (i)
#             4028 BINARY_SUBSCR
#             4030 CALL_METHOD          (2 positional arguments)
#             4032 POP_TOP
#             4034 JUMP_FORWARD         (to 4044)
# 
# 13579:     >> 4036 LOAD_FAST            (btn)
#             4038 LOAD_METHOD          (hide)
#             4040 CALL_METHOD          (0 positional arguments)
#             4042 POP_TOP
#          >> 4044 EXTENDED_ARG         (3840)
#             4046 JUMP_ABSOLUTE        (to 3980)
#          >> 4048 EXTENDED_ARG         (1280)
#             4050 JUMP_FORWARD         (to 5556)
# 
# 13581:     >> 4052 LOAD_FAST            (effect)
#             4054 LOAD_CONST           ("type")
#             4056 BINARY_SUBSCR
#             4058 LOAD_CONST           ("lasers")
#             4060 COMPARE_OP           (==)
#             4062 EXTENDED_ARG         (4096)
#             4064 POP_JUMP_IF_FALSE    (to 4148)
# 
# 13582:        4066 LOAD_GLOBAL          (enumerate)
#             4068 LOAD_FAST            (self)
#             4070 LOAD_ATTR            (color_btns)
#             4072 CALL_FUNCTION        (1 positional argument)
#             4074 GET_ITER
#          >> 4076 FOR_ITER             (to 4144)
#             4078 UNPACK_SEQUENCE      2
#             4080 STORE_FAST           (i)
#             4082 STORE_FAST           (btn)
# 
# 13583:        4084 LOAD_FAST            (i)
#             4086 LOAD_GLOBAL          (len)
#             4088 LOAD_FAST            (effect)
#             4090 LOAD_CONST           ("colors")
#             4092 BINARY_SUBSCR
#             4094 CALL_FUNCTION        (1 positional argument)
#             4096 COMPARE_OP           (<)
#             4098 EXTENDED_ARG         (4096)
#             4100 POP_JUMP_IF_FALSE    (to 4132)
# 
# 13584:        4102 LOAD_FAST            (btn)
#             4104 LOAD_METHOD          (show)
#             4106 CALL_METHOD          (0 positional arguments)
#             4108 POP_TOP
# 
# 13585:        4110 LOAD_FAST            (self)
#             4112 LOAD_METHOD          (set_button_color)
#             4114 LOAD_FAST            (btn)
#             4116 LOAD_FAST            (effect)
#             4118 LOAD_CONST           ("colors")
#             4120 BINARY_SUBSCR
#             4122 LOAD_FAST            (i)
#             4124 BINARY_SUBSCR
#             4126 CALL_METHOD          (2 positional arguments)
#             4128 POP_TOP
#             4130 JUMP_FORWARD         (to 4140)
# 
# 13587:     >> 4132 LOAD_FAST            (btn)
#             4134 LOAD_METHOD          (hide)
#             4136 CALL_METHOD          (0 positional arguments)
#             4138 POP_TOP
#          >> 4140 EXTENDED_ARG         (3840)
#             4142 JUMP_ABSOLUTE        (to 4076)
#          >> 4144 EXTENDED_ARG         (1280)
#             4146 JUMP_FORWARD         (to 5556)
# 
# 13588:     >> 4148 LOAD_FAST            (effect)
#             4150 LOAD_CONST           ("type")
#             4152 BINARY_SUBSCR
#             4154 LOAD_CONST           ("lava_lamp")
#             4156 COMPARE_OP           (==)
#             4158 EXTENDED_ARG         (4096)
#             4160 POP_JUMP_IF_FALSE    (to 4236)
# 
# 13589:        4162 LOAD_GLOBAL          (enumerate)
#             4164 LOAD_FAST            (self)
#             4166 LOAD_ATTR            (color_btns)
#             4168 CALL_FUNCTION        (1 positional argument)
#             4170 GET_ITER
#          >> 4172 FOR_ITER             (to 4232)
#             4174 UNPACK_SEQUENCE      2
#             4176 STORE_FAST           (i)
#             4178 STORE_FAST           (btn)
# 
# 13590:        4180 LOAD_FAST            (i)
#             4182 LOAD_CONST           (3)
#             4184 COMPARE_OP           (<)
#             4186 EXTENDED_ARG         (4096)
#             4188 POP_JUMP_IF_FALSE    (to 4220)
# 
# 13591:        4190 LOAD_FAST            (btn)
#             4192 LOAD_METHOD          (show)
#             4194 CALL_METHOD          (0 positional arguments)
#             4196 POP_TOP
# 
# 13592:        4198 LOAD_FAST            (self)
#             4200 LOAD_METHOD          (set_button_color)
#             4202 LOAD_FAST            (btn)
#             4204 LOAD_FAST            (effect)
#             4206 LOAD_CONST           ("colors")
#             4208 BINARY_SUBSCR
#             4210 LOAD_FAST            (i)
#             4212 BINARY_SUBSCR
#             4214 CALL_METHOD          (2 positional arguments)
#             4216 POP_TOP
#             4218 JUMP_FORWARD         (to 4228)
# 
# 13594:     >> 4220 LOAD_FAST            (btn)
#             4222 LOAD_METHOD          (hide)
#             4224 CALL_METHOD          (0 positional arguments)
#             4226 POP_TOP
#          >> 4228 EXTENDED_ARG         (4096)
#             4230 JUMP_ABSOLUTE        (to 4172)
#          >> 4232 EXTENDED_ARG         (1280)
#             4234 JUMP_FORWARD         (to 5556)
# 
# 13595:     >> 4236 LOAD_FAST            (effect)
#             4238 LOAD_CONST           ("type")
#             4240 BINARY_SUBSCR
#             4242 LOAD_CONST           ("mario")
#             4244 COMPARE_OP           (==)
#             4246 EXTENDED_ARG         (4096)
#             4248 POP_JUMP_IF_FALSE    (to 4332)
# 
# 13596:        4250 LOAD_GLOBAL          (enumerate)
#             4252 LOAD_FAST            (self)
#             4254 LOAD_ATTR            (color_btns)
#             4256 CALL_FUNCTION        (1 positional argument)
#             4258 GET_ITER
#          >> 4260 FOR_ITER             (to 4328)
#             4262 UNPACK_SEQUENCE      2
#             4264 STORE_FAST           (i)
#             4266 STORE_FAST           (btn)
# 
# 13597:        4268 LOAD_FAST            (i)
#             4270 LOAD_GLOBAL          (len)
#             4272 LOAD_FAST            (effect)
#             4274 LOAD_CONST           ("colors")
#             4276 BINARY_SUBSCR
#             4278 CALL_FUNCTION        (1 positional argument)
#             4280 COMPARE_OP           (<)
#             4282 EXTENDED_ARG         (4096)
#             4284 POP_JUMP_IF_FALSE    (to 4316)
# 
# 13598:        4286 LOAD_FAST            (btn)
#             4288 LOAD_METHOD          (show)
#             4290 CALL_METHOD          (0 positional arguments)
#             4292 POP_TOP
# 
# 13599:        4294 LOAD_FAST            (self)
#             4296 LOAD_METHOD          (set_button_color)
#             4298 LOAD_FAST            (btn)
#             4300 LOAD_FAST            (effect)
#             4302 LOAD_CONST           ("colors")
#             4304 BINARY_SUBSCR
#             4306 LOAD_FAST            (i)
#             4308 BINARY_SUBSCR
#             4310 CALL_METHOD          (2 positional arguments)
#             4312 POP_TOP
#             4314 JUMP_FORWARD         (to 4324)
# 
# 13601:     >> 4316 LOAD_FAST            (btn)
#             4318 LOAD_METHOD          (hide)
#             4320 CALL_METHOD          (0 positional arguments)
#             4322 POP_TOP
#          >> 4324 EXTENDED_ARG         (4096)
#             4326 JUMP_ABSOLUTE        (to 4260)
#          >> 4328 EXTENDED_ARG         (1024)
#             4330 JUMP_FORWARD         (to 5556)
# 
# 13602:     >> 4332 LOAD_FAST            (effect)
#             4334 LOAD_CONST           ("type")
#             4336 BINARY_SUBSCR
#             4338 LOAD_CONST           ("multiverse_simple")
#             4340 COMPARE_OP           (==)
#             4342 EXTENDED_ARG         (4352)
#             4344 POP_JUMP_IF_FALSE    (to 4420)
# 
# 13603:        4346 LOAD_GLOBAL          (enumerate)
#             4348 LOAD_FAST            (self)
#             4350 LOAD_ATTR            (color_btns)
#             4352 CALL_FUNCTION        (1 positional argument)
#             4354 GET_ITER
#          >> 4356 FOR_ITER             (to 4416)
#             4358 UNPACK_SEQUENCE      2
#             4360 STORE_FAST           (i)
#             4362 STORE_FAST           (btn)
# 
# 13604:        4364 LOAD_FAST            (i)
#             4366 LOAD_CONST           (2)
#             4368 COMPARE_OP           (<)
#             4370 EXTENDED_ARG         (4352)
#             4372 POP_JUMP_IF_FALSE    (to 4404)
# 
# 13605:        4374 LOAD_FAST            (btn)
#             4376 LOAD_METHOD          (show)
#             4378 CALL_METHOD          (0 positional arguments)
#             4380 POP_TOP
# 
# 13606:        4382 LOAD_FAST            (self)
#             4384 LOAD_METHOD          (set_button_color)
#             4386 LOAD_FAST            (btn)
#             4388 LOAD_FAST            (effect)
#             4390 LOAD_CONST           ("colors")
#             4392 BINARY_SUBSCR
#             4394 LOAD_FAST            (i)
#             4396 BINARY_SUBSCR
#             4398 CALL_METHOD          (2 positional arguments)
#             4400 POP_TOP
#             4402 JUMP_FORWARD         (to 4412)
# 
# 13608:     >> 4404 LOAD_FAST            (btn)
#             4406 LOAD_METHOD          (hide)
#             4408 CALL_METHOD          (0 positional arguments)
#             4410 POP_TOP
#          >> 4412 EXTENDED_ARG         (4352)
#             4414 JUMP_ABSOLUTE        (to 4356)
#          >> 4416 EXTENDED_ARG         (1024)
#             4418 JUMP_FORWARD         (to 5556)
# 
# 13609:     >> 4420 LOAD_FAST            (effect)
#             4422 LOAD_CONST           ("type")
#             4424 BINARY_SUBSCR
#             4426 LOAD_CONST           ("cosmic_nebula")
#             4428 COMPARE_OP           (==)
#             4430 EXTENDED_ARG         (4352)
#             4432 POP_JUMP_IF_FALSE    (to 4508)
# 
# 13610:        4434 LOAD_GLOBAL          (enumerate)
#             4436 LOAD_FAST            (self)
#             4438 LOAD_ATTR            (color_btns)
#             4440 CALL_FUNCTION        (1 positional argument)
#             4442 GET_ITER
#          >> 4444 FOR_ITER             (to 4504)
#             4446 UNPACK_SEQUENCE      2
#             4448 STORE_FAST           (i)
#             4450 STORE_FAST           (btn)
# 
# 13611:        4452 LOAD_FAST            (i)
#             4454 LOAD_CONST           (4)
#             4456 COMPARE_OP           (<)
#             4458 EXTENDED_ARG         (4352)
#             4460 POP_JUMP_IF_FALSE    (to 4492)
# 
# 13612:        4462 LOAD_FAST            (btn)
#             4464 LOAD_METHOD          (show)
#             4466 CALL_METHOD          (0 positional arguments)
#             4468 POP_TOP
# 
# 13613:        4470 LOAD_FAST            (self)
#             4472 LOAD_METHOD          (set_button_color)
#             4474 LOAD_FAST            (btn)
#             4476 LOAD_FAST            (effect)
#             4478 LOAD_CONST           ("colors")
#             4480 BINARY_SUBSCR
#             4482 LOAD_FAST            (i)
#             4484 BINARY_SUBSCR
#             4486 CALL_METHOD          (2 positional arguments)
#             4488 POP_TOP
#             4490 JUMP_FORWARD         (to 4500)
# 
# 13615:     >> 4492 LOAD_FAST            (btn)
#             4494 LOAD_METHOD          (hide)
#             4496 CALL_METHOD          (0 positional arguments)
#             4498 POP_TOP
#          >> 4500 EXTENDED_ARG         (4352)
#             4502 JUMP_ABSOLUTE        (to 4444)
#          >> 4504 EXTENDED_ARG         (1024)
#             4506 JUMP_FORWARD         (to 5556)
# 
# 13616:     >> 4508 LOAD_FAST            (effect)
#             4510 LOAD_CONST           ("type")
#             4512 BINARY_SUBSCR
#             4514 LOAD_CONST           ("neon_grid")
#             4516 COMPARE_OP           (==)
#             4518 EXTENDED_ARG         (4352)
#             4520 POP_JUMP_IF_FALSE    (to 4596)
# 
# 13617:        4522 LOAD_GLOBAL          (enumerate)
#             4524 LOAD_FAST            (self)
#             4526 LOAD_ATTR            (color_btns)
#             4528 CALL_FUNCTION        (1 positional argument)
#             4530 GET_ITER
#          >> 4532 FOR_ITER             (to 4592)
#             4534 UNPACK_SEQUENCE      2
#             4536 STORE_FAST           (i)
#             4538 STORE_FAST           (btn)
# 
# 13618:        4540 LOAD_FAST            (i)
#             4542 LOAD_CONST           (4)
#             4544 COMPARE_OP           (<)
#             4546 EXTENDED_ARG         (4352)
#             4548 POP_JUMP_IF_FALSE    (to 4580)
# 
# 13619:        4550 LOAD_FAST            (btn)
#             4552 LOAD_METHOD          (show)
#             4554 CALL_METHOD          (0 positional arguments)
#             4556 POP_TOP
# 
# 13620:        4558 LOAD_FAST            (self)
#             4560 LOAD_METHOD          (set_button_color)
#             4562 LOAD_FAST            (btn)
#             4564 LOAD_FAST            (effect)
#             4566 LOAD_CONST           ("colors")
#             4568 BINARY_SUBSCR
#             4570 LOAD_FAST            (i)
#             4572 BINARY_SUBSCR
#             4574 CALL_METHOD          (2 positional arguments)
#             4576 POP_TOP
#             4578 JUMP_FORWARD         (to 4588)
# 
# 13622:     >> 4580 LOAD_FAST            (btn)
#             4582 LOAD_METHOD          (hide)
#             4584 CALL_METHOD          (0 positional arguments)
#             4586 POP_TOP
#          >> 4588 EXTENDED_ARG         (4352)
#             4590 JUMP_ABSOLUTE        (to 4532)
#          >> 4592 EXTENDED_ARG         (768)
#             4594 JUMP_FORWARD         (to 5556)
# 
# 13623:     >> 4596 LOAD_FAST            (effect)
#             4598 LOAD_CONST           ("type")
#             4600 BINARY_SUBSCR
#             4602 LOAD_CONST           ("neurolink")
#             4604 COMPARE_OP           (==)
#             4606 EXTENDED_ARG         (4608)
#             4608 POP_JUMP_IF_FALSE    (to 4684)
# 
# 13624:        4610 LOAD_GLOBAL          (enumerate)
#             4612 LOAD_FAST            (self)
#             4614 LOAD_ATTR            (color_btns)
#             4616 CALL_FUNCTION        (1 positional argument)
#             4618 GET_ITER
#          >> 4620 FOR_ITER             (to 4680)
#             4622 UNPACK_SEQUENCE      2
#             4624 STORE_FAST           (i)
#             4626 STORE_FAST           (btn)
# 
# 13625:        4628 LOAD_FAST            (i)
#             4630 LOAD_CONST           (3)
#             4632 COMPARE_OP           (<)
#             4634 EXTENDED_ARG         (4608)
#             4636 POP_JUMP_IF_FALSE    (to 4668)
# 
# 13626:        4638 LOAD_FAST            (btn)
#             4640 LOAD_METHOD          (show)
#             4642 CALL_METHOD          (0 positional arguments)
#             4644 POP_TOP
# 
# 13627:        4646 LOAD_FAST            (self)
#             4648 LOAD_METHOD          (set_button_color)
#             4650 LOAD_FAST            (btn)
#             4652 LOAD_FAST            (effect)
#             4654 LOAD_CONST           ("colors")
#             4656 BINARY_SUBSCR
#             4658 LOAD_FAST            (i)
#             4660 BINARY_SUBSCR
#             4662 CALL_METHOD          (2 positional arguments)
#             4664 POP_TOP
#             4666 JUMP_FORWARD         (to 4676)
# 
# 13629:     >> 4668 LOAD_FAST            (btn)
#             4670 LOAD_METHOD          (hide)
#             4672 CALL_METHOD          (0 positional arguments)
#             4674 POP_TOP
#          >> 4676 EXTENDED_ARG         (4608)
#             4678 JUMP_ABSOLUTE        (to 4620)
#          >> 4680 EXTENDED_ARG         (768)
#             4682 JUMP_FORWARD         (to 5556)
# 
# 13630:     >> 4684 LOAD_FAST            (effect)
#             4686 LOAD_CONST           ("type")
#             4688 BINARY_SUBSCR
#             4690 LOAD_CONST           ("new_years")
#             4692 COMPARE_OP           (==)
#             4694 EXTENDED_ARG         (4608)
#             4696 POP_JUMP_IF_FALSE    (to 4772)
# 
# 13631:        4698 LOAD_GLOBAL          (enumerate)
#             4700 LOAD_FAST            (self)
#             4702 LOAD_ATTR            (color_btns)
#             4704 CALL_FUNCTION        (1 positional argument)
#             4706 GET_ITER
#          >> 4708 FOR_ITER             (to 4768)
#             4710 UNPACK_SEQUENCE      2
#             4712 STORE_FAST           (i)
#             4714 STORE_FAST           (btn)
# 
# 13632:        4716 LOAD_FAST            (i)
#             4718 LOAD_CONST           (3)
#             4720 COMPARE_OP           (<)
#             4722 EXTENDED_ARG         (4608)
#             4724 POP_JUMP_IF_FALSE    (to 4756)
# 
# 13633:        4726 LOAD_FAST            (btn)
#             4728 LOAD_METHOD          (show)
#             4730 CALL_METHOD          (0 positional arguments)
#             4732 POP_TOP
# 
# 13634:        4734 LOAD_FAST            (self)
#             4736 LOAD_METHOD          (set_button_color)
#             4738 LOAD_FAST            (btn)
#             4740 LOAD_FAST            (effect)
#             4742 LOAD_CONST           ("colors")
#             4744 BINARY_SUBSCR
#             4746 LOAD_FAST            (i)
#             4748 BINARY_SUBSCR
#             4750 CALL_METHOD          (2 positional arguments)
#             4752 POP_TOP
#             4754 JUMP_FORWARD         (to 4764)
# 
# 13636:     >> 4756 LOAD_FAST            (btn)
#             4758 LOAD_METHOD          (hide)
#             4760 CALL_METHOD          (0 positional arguments)
#             4762 POP_TOP
#          >> 4764 EXTENDED_ARG         (4608)
#             4766 JUMP_ABSOLUTE        (to 4708)
#          >> 4768 EXTENDED_ARG         (768)
#             4770 JUMP_FORWARD         (to 5556)
# 
# 13637:     >> 4772 LOAD_FAST            (effect)
#             4774 LOAD_CONST           ("type")
#             4776 BINARY_SUBSCR
#             4778 LOAD_CONST           ("paint_splatters")
#             4780 COMPARE_OP           (==)
#             4782 EXTENDED_ARG         (4608)
#             4784 POP_JUMP_IF_FALSE    (to 4860)
# 
# 13638:        4786 LOAD_GLOBAL          (enumerate)
#             4788 LOAD_FAST            (self)
#             4790 LOAD_ATTR            (color_btns)
#             4792 CALL_FUNCTION        (1 positional argument)
#             4794 GET_ITER
#          >> 4796 FOR_ITER             (to 4856)
#             4798 UNPACK_SEQUENCE      2
#             4800 STORE_FAST           (i)
#             4802 STORE_FAST           (btn)
# 
# 13639:        4804 LOAD_FAST            (i)
#             4806 LOAD_CONST           (3)
#             4808 COMPARE_OP           (<)
#             4810 EXTENDED_ARG         (4608)
#             4812 POP_JUMP_IF_FALSE    (to 4844)
# 
# 13640:        4814 LOAD_FAST            (btn)
#             4816 LOAD_METHOD          (show)
#             4818 CALL_METHOD          (0 positional arguments)
#             4820 POP_TOP
# 
# 13641:        4822 LOAD_FAST            (self)
#             4824 LOAD_METHOD          (set_button_color)
#             4826 LOAD_FAST            (btn)
#             4828 LOAD_FAST            (effect)
#             4830 LOAD_CONST           ("colors")
#             4832 BINARY_SUBSCR
#             4834 LOAD_FAST            (i)
#             4836 BINARY_SUBSCR
#             4838 CALL_METHOD          (2 positional arguments)
#             4840 POP_TOP
#             4842 JUMP_FORWARD         (to 4852)
# 
# 13643:     >> 4844 LOAD_FAST            (btn)
#             4846 LOAD_METHOD          (hide)
#             4848 CALL_METHOD          (0 positional arguments)
#             4850 POP_TOP
#          >> 4852 EXTENDED_ARG         (4608)
#             4854 JUMP_ABSOLUTE        (to 4796)
#          >> 4856 EXTENDED_ARG         (512)
#             4858 JUMP_FORWARD         (to 5556)
# 
# 13644:     >> 4860 LOAD_FAST            (effect)
#             4862 LOAD_CONST           ("type")
#             4864 BINARY_SUBSCR
#             4866 LOAD_CONST           ("party_parrot")
#             4868 COMPARE_OP           (==)
#             4870 EXTENDED_ARG         (4864)
#             4872 POP_JUMP_IF_FALSE    (to 4948)
# 
# 13645:        4874 LOAD_GLOBAL          (enumerate)
#             4876 LOAD_FAST            (self)
#             4878 LOAD_ATTR            (color_btns)
#             4880 CALL_FUNCTION        (1 positional argument)
#             4882 GET_ITER
#          >> 4884 FOR_ITER             (to 4944)
#             4886 UNPACK_SEQUENCE      2
#             4888 STORE_FAST           (i)
#             4890 STORE_FAST           (btn)
# 
# 13646:        4892 LOAD_FAST            (i)
#             4894 LOAD_CONST           (4)
#             4896 COMPARE_OP           (<)
#             4898 EXTENDED_ARG         (4864)
#             4900 POP_JUMP_IF_FALSE    (to 4932)
# 
# 13647:        4902 LOAD_FAST            (btn)
#             4904 LOAD_METHOD          (show)
#             4906 CALL_METHOD          (0 positional arguments)
#             4908 POP_TOP
# 
# 13648:        4910 LOAD_FAST            (self)
#             4912 LOAD_METHOD          (set_button_color)
#             4914 LOAD_FAST            (btn)
#             4916 LOAD_FAST            (effect)
#             4918 LOAD_CONST           ("colors")
#             4920 BINARY_SUBSCR
#             4922 LOAD_FAST            (i)
#             4924 BINARY_SUBSCR
#             4926 CALL_METHOD          (2 positional arguments)
#             4928 POP_TOP
#             4930 JUMP_FORWARD         (to 4940)
# 
# 13650:     >> 4932 LOAD_FAST            (btn)
#             4934 LOAD_METHOD          (hide)
#             4936 CALL_METHOD          (0 positional arguments)
#             4938 POP_TOP
#          >> 4940 EXTENDED_ARG         (4864)
#             4942 JUMP_ABSOLUTE        (to 4884)
#          >> 4944 EXTENDED_ARG         (512)
#             4946 JUMP_FORWARD         (to 5556)
# 
# 13651:     >> 4948 LOAD_FAST            (effect)
#             4950 LOAD_CONST           ("type")
#             4952 BINARY_SUBSCR
#             4954 LOAD_CONST           ("pixel_wave")
#             4956 COMPARE_OP           (==)
#             4958 EXTENDED_ARG         (4864)
#             4960 POP_JUMP_IF_FALSE    (to 5036)
# 
# 13652:        4962 LOAD_GLOBAL          (enumerate)
#             4964 LOAD_FAST            (self)
#             4966 LOAD_ATTR            (color_btns)
#             4968 CALL_FUNCTION        (1 positional argument)
#             4970 GET_ITER
#          >> 4972 FOR_ITER             (to 5032)
#             4974 UNPACK_SEQUENCE      2
#             4976 STORE_FAST           (i)
#             4978 STORE_FAST           (btn)
# 
# 13653:        4980 LOAD_FAST            (i)
#             4982 LOAD_CONST           (4)
#             4984 COMPARE_OP           (<)
#             4986 EXTENDED_ARG         (4864)
#             4988 POP_JUMP_IF_FALSE    (to 5020)
# 
# 13654:        4990 LOAD_FAST            (btn)
#             4992 LOAD_METHOD          (show)
#             4994 CALL_METHOD          (0 positional arguments)
#             4996 POP_TOP
# 
# 13655:        4998 LOAD_FAST            (self)
#             5000 LOAD_METHOD          (set_button_color)
#             5002 LOAD_FAST            (btn)
#             5004 LOAD_FAST            (effect)
#             5006 LOAD_CONST           ("colors")
#             5008 BINARY_SUBSCR
#             5010 LOAD_FAST            (i)
#             5012 BINARY_SUBSCR
#             5014 CALL_METHOD          (2 positional arguments)
#             5016 POP_TOP
#             5018 JUMP_FORWARD         (to 5028)
# 
# 13657:     >> 5020 LOAD_FAST            (btn)
#             5022 LOAD_METHOD          (hide)
#             5024 CALL_METHOD          (0 positional arguments)
#             5026 POP_TOP
#          >> 5028 EXTENDED_ARG         (4864)
#             5030 JUMP_ABSOLUTE        (to 4972)
#          >> 5032 EXTENDED_ARG         (512)
#             5034 JUMP_FORWARD         (to 5556)
# 
# 13658:     >> 5036 LOAD_FAST            (effect)
#             5038 LOAD_CONST           ("type")
#             5040 BINARY_SUBSCR
#             5042 LOAD_CONST           ("planetary_orbit")
#             5044 COMPARE_OP           (==)
#             5046 EXTENDED_ARG         (5120)
#             5048 POP_JUMP_IF_FALSE    (to 5124)
# 
# 13659:        5050 LOAD_GLOBAL          (enumerate)
#             5052 LOAD_FAST            (self)
#             5054 LOAD_ATTR            (color_btns)
#             5056 CALL_FUNCTION        (1 positional argument)
#             5058 GET_ITER
#          >> 5060 FOR_ITER             (to 5120)
#             5062 UNPACK_SEQUENCE      2
#             5064 STORE_FAST           (i)
#             5066 STORE_FAST           (btn)
# 
# 13660:        5068 LOAD_FAST            (i)
#             5070 LOAD_CONST           (4)
#             5072 COMPARE_OP           (<)
#             5074 EXTENDED_ARG         (4864)
#             5076 POP_JUMP_IF_FALSE    (to 5108)
# 
# 13661:        5078 LOAD_FAST            (btn)
#             5080 LOAD_METHOD          (show)
#             5082 CALL_METHOD          (0 positional arguments)
#             5084 POP_TOP
# 
# 13662:        5086 LOAD_FAST            (self)
#             5088 LOAD_METHOD          (set_button_color)
#             5090 LOAD_FAST            (btn)
#             5092 LOAD_FAST            (effect)
#             5094 LOAD_CONST           ("colors")
#             5096 BINARY_SUBSCR
#             5098 LOAD_FAST            (i)
#             5100 BINARY_SUBSCR
#             5102 CALL_METHOD          (2 positional arguments)
#             5104 POP_TOP
#             5106 JUMP_FORWARD         (to 5116)
# 
# 13664:     >> 5108 LOAD_FAST            (btn)
#             5110 LOAD_METHOD          (hide)
#             5112 CALL_METHOD          (0 positional arguments)
#             5114 POP_TOP
#          >> 5116 EXTENDED_ARG         (4864)
#             5118 JUMP_ABSOLUTE        (to 5060)
#          >> 5120 EXTENDED_ARG         (256)
#             5122 JUMP_FORWARD         (to 5556)
# 
# 13665:     >> 5124 LOAD_FAST            (effect)
#             5126 LOAD_CONST           ("type")
#             5128 BINARY_SUBSCR
#             5130 LOAD_CONST           ("plasma_flow")
#             5132 COMPARE_OP           (==)
#             5134 EXTENDED_ARG         (5120)
#             5136 POP_JUMP_IF_FALSE    (to 5212)
# 
# 13666:        5138 LOAD_GLOBAL          (enumerate)
#             5140 LOAD_FAST            (self)
#             5142 LOAD_ATTR            (color_btns)
#             5144 CALL_FUNCTION        (1 positional argument)
#             5146 GET_ITER
#          >> 5148 FOR_ITER             (to 5208)
#             5150 UNPACK_SEQUENCE      2
#             5152 STORE_FAST           (i)
#             5154 STORE_FAST           (btn)
# 
# 13667:        5156 LOAD_FAST            (i)
#             5158 LOAD_CONST           (4)
#             5160 COMPARE_OP           (<)
#             5162 EXTENDED_ARG         (5120)
#             5164 POP_JUMP_IF_FALSE    (to 5196)
# 
# 13668:        5166 LOAD_FAST            (btn)
#             5168 LOAD_METHOD          (show)
#             5170 CALL_METHOD          (0 positional arguments)
#             5172 POP_TOP
# 
# 13669:        5174 LOAD_FAST            (self)
#             5176 LOAD_METHOD          (set_button_color)
#             5178 LOAD_FAST            (btn)
#             5180 LOAD_FAST            (effect)
#             5182 LOAD_CONST           ("colors")
#             5184 BINARY_SUBSCR
#             5186 LOAD_FAST            (i)
#             5188 BINARY_SUBSCR
#             5190 CALL_METHOD          (2 positional arguments)
#             5192 POP_TOP
#             5194 JUMP_FORWARD         (to 5204)
# 
# 13671:     >> 5196 LOAD_FAST            (btn)
#             5198 LOAD_METHOD          (hide)
#             5200 CALL_METHOD          (0 positional arguments)
#             5202 POP_TOP
#          >> 5204 EXTENDED_ARG         (5120)
#             5206 JUMP_ABSOLUTE        (to 5148)
#          >> 5208 EXTENDED_ARG         (256)
#             5210 JUMP_FORWARD         (to 5556)
# 
# 13672:     >> 5212 LOAD_FAST            (effect)
#             5214 LOAD_CONST           ("type")
#             5216 BINARY_SUBSCR
#             5218 LOAD_CONST           ("poison_max")
#             5220 COMPARE_OP           (==)
#             5222 EXTENDED_ARG         (5120)
#             5224 POP_JUMP_IF_FALSE    (to 5300)
# 
# 13673:        5226 LOAD_GLOBAL          (enumerate)
#             5228 LOAD_FAST            (self)
#             5230 LOAD_ATTR            (color_btns)
#             5232 CALL_FUNCTION        (1 positional argument)
#             5234 GET_ITER
#          >> 5236 FOR_ITER             (to 5296)
#             5238 UNPACK_SEQUENCE      2
#             5240 STORE_FAST           (i)
#             5242 STORE_FAST           (btn)
# 
# 13674:        5244 LOAD_FAST            (i)
#             5246 LOAD_CONST           (4)
#             5248 COMPARE_OP           (<)
#             5250 EXTENDED_ARG         (5120)
#             5252 POP_JUMP_IF_FALSE    (to 5284)
# 
# 13675:        5254 LOAD_FAST            (btn)
#             5256 LOAD_METHOD          (show)
#             5258 CALL_METHOD          (0 positional arguments)
#             5260 POP_TOP
# 
# 13676:        5262 LOAD_FAST            (self)
#             5264 LOAD_METHOD          (set_button_color)
#             5266 LOAD_FAST            (btn)
#             5268 LOAD_FAST            (effect)
#             5270 LOAD_CONST           ("colors")
#             5272 BINARY_SUBSCR
#             5274 LOAD_FAST            (i)
#             5276 BINARY_SUBSCR
#             5278 CALL_METHOD          (2 positional arguments)
#             5280 POP_TOP
#             5282 JUMP_FORWARD         (to 5292)
# 
# 13678:     >> 5284 LOAD_FAST            (btn)
#             5286 LOAD_METHOD          (hide)
#             5288 CALL_METHOD          (0 positional arguments)
#             5290 POP_TOP
#          >> 5292 EXTENDED_ARG         (5120)
#             5294 JUMP_ABSOLUTE        (to 5236)
#          >> 5296 EXTENDED_ARG         (256)
#             5298 JUMP_FORWARD         (to 5556)
# 
# 13679:     >> 5300 LOAD_FAST            (effect)
#             5302 LOAD_CONST           ("type")
#             5304 BINARY_SUBSCR
#             5306 LOAD_CONST           ("rainbow_tap")
#             5308 COMPARE_OP           (==)
#             5310 EXTENDED_ARG         (5376)
#             5312 POP_JUMP_IF_FALSE    (to 5386)
# 
# 13680:        5314 LOAD_GLOBAL          (enumerate)
#             5316 LOAD_FAST            (self)
#             5318 LOAD_ATTR            (color_btns)
#             5320 CALL_FUNCTION        (1 positional argument)
#             5322 GET_ITER
#          >> 5324 FOR_ITER             (to 5384)
#             5326 UNPACK_SEQUENCE      2
#             5328 STORE_FAST           (i)
#             5330 STORE_FAST           (btn)
# 
# 13681:        5332 LOAD_FAST            (i)
#             5334 LOAD_CONST           (4)
#             5336 COMPARE_OP           (<)
#             5338 EXTENDED_ARG         (5120)
#             5340 POP_JUMP_IF_FALSE    (to 5372)
# 
# 13682:        5342 LOAD_FAST            (btn)
#             5344 LOAD_METHOD          (show)
#             5346 CALL_METHOD          (0 positional arguments)
#             5348 POP_TOP
# 
# 13683:        5350 LOAD_FAST            (self)
#             5352 LOAD_METHOD          (set_button_color)
#             5354 LOAD_FAST            (btn)
#             5356 LOAD_FAST            (effect)
#             5358 LOAD_CONST           ("colors")
#             5360 BINARY_SUBSCR
#             5362 LOAD_FAST            (i)
#             5364 BINARY_SUBSCR
#             5366 CALL_METHOD          (2 positional arguments)
#             5368 POP_TOP
#             5370 JUMP_FORWARD         (to 5380)
# 
# 13685:     >> 5372 LOAD_FAST            (btn)
#             5374 LOAD_METHOD          (hide)
#             5376 CALL_METHOD          (0 positional arguments)
#             5378 POP_TOP
#          >> 5380 EXTENDED_ARG         (5120)
#             5382 JUMP_ABSOLUTE        (to 5324)
#          >> 5384 JUMP_FORWARD         (to 5556)
# 
# 13686:     >> 5386 LOAD_FAST            (effect)
#             5388 LOAD_CONST           ("type")
#             5390 BINARY_SUBSCR
#             5392 LOAD_CONST           ("resident_evil")
#             5394 COMPARE_OP           (==)
#             5396 EXTENDED_ARG         (5376)
#             5398 POP_JUMP_IF_FALSE    (to 5472)
# 
# 13687:        5400 LOAD_GLOBAL          (enumerate)
#             5402 LOAD_FAST            (self)
#             5404 LOAD_ATTR            (color_btns)
#             5406 CALL_FUNCTION        (1 positional argument)
#             5408 GET_ITER
#          >> 5410 FOR_ITER             (to 5470)
#             5412 UNPACK_SEQUENCE      2
#             5414 STORE_FAST           (i)
#             5416 STORE_FAST           (btn)
# 
# 13688:        5418 LOAD_FAST            (i)
#             5420 LOAD_CONST           (3)
#             5422 COMPARE_OP           (<)
#             5424 EXTENDED_ARG         (5376)
#             5426 POP_JUMP_IF_FALSE    (to 5458)
# 
# 13689:        5428 LOAD_FAST            (btn)
#             5430 LOAD_METHOD          (show)
#             5432 CALL_METHOD          (0 positional arguments)
#             5434 POP_TOP
# 
# 13690:        5436 LOAD_FAST            (self)
#             5438 LOAD_METHOD          (set_button_color)
#             5440 LOAD_FAST            (btn)
#             5442 LOAD_FAST            (effect)
#             5444 LOAD_CONST           ("colors")
#             5446 BINARY_SUBSCR
#             5448 LOAD_FAST            (i)
#             5450 BINARY_SUBSCR
#             5452 CALL_METHOD          (2 positional arguments)
#             5454 POP_TOP
#             5456 JUMP_FORWARD         (to 5466)
# 
# 13692:     >> 5458 LOAD_FAST            (btn)
#             5460 LOAD_METHOD          (hide)
#             5462 CALL_METHOD          (0 positional arguments)
#             5464 POP_TOP
#          >> 5466 EXTENDED_ARG         (5376)
#             5468 JUMP_ABSOLUTE        (to 5410)
#          >> 5470 JUMP_FORWARD         (to 5556)
# 
# 13693:     >> 5472 LOAD_FAST            (effect)
#             5474 LOAD_CONST           ("type")
#             5476 BINARY_SUBSCR
#             5478 LOAD_CONST           ("rick_morty")
#             5480 COMPARE_OP           (==)
#             5482 EXTENDED_ARG         (5376)
#             5484 POP_JUMP_IF_FALSE    (to 5556)
# 
# 13694:        5486 LOAD_GLOBAL          (enumerate)
#             5488 LOAD_FAST            (self)
#             5490 LOAD_ATTR            (color_btns)
#             5492 CALL_FUNCTION        (1 positional argument)
#             5494 GET_ITER
#          >> 5496 FOR_ITER             (to 5556)
#             5498 UNPACK_SEQUENCE      2
#             5500 STORE_FAST           (i)
#             5502 STORE_FAST           (btn)
# 
# 13695:        5504 LOAD_FAST            (i)
#             5506 LOAD_CONST           (4)
#             5508 COMPARE_OP           (<)
#             5510 EXTENDED_ARG         (5376)
#             5512 POP_JUMP_IF_FALSE    (to 5544)
# 
# 13696:        5514 LOAD_FAST            (btn)
#             5516 LOAD_METHOD          (show)
#             5518 CALL_METHOD          (0 positional arguments)
#             5520 POP_TOP
# 
# 13697:        5522 LOAD_FAST            (self)
#             5524 LOAD_METHOD          (set_button_color)
#             5526 LOAD_FAST            (btn)
#             5528 LOAD_FAST            (effect)
#             5530 LOAD_CONST           ("colors")
#             5532 BINARY_SUBSCR
#             5534 LOAD_FAST            (i)
#             5536 BINARY_SUBSCR
#             5538 CALL_METHOD          (2 positional arguments)
#             5540 POP_TOP
#             5542 JUMP_FORWARD         (to 5552)
# 
# 13699:     >> 5544 LOAD_FAST            (btn)
#             5546 LOAD_METHOD          (hide)
#             5548 CALL_METHOD          (0 positional arguments)
#             5550 POP_TOP
#          >> 5552 EXTENDED_ARG         (5376)
#             5554 JUMP_ABSOLUTE        (to 5496)
# 
# 13705:     >> 5556 LOAD_FAST            (self)
#             5558 LOAD_ATTR            (canvas_scene)
#             5560 LOAD_ATTR            (selected_icon)
#             5562 EXTENDED_ARG         (5632)
#             5564 POP_JUMP_IF_FALSE    (to 5674)
# 
# 13706:        5566 LOAD_FAST            (self)
#             5568 LOAD_ATTR            (orientation_group)
#             5570 LOAD_METHOD          (show)
#             5572 CALL_METHOD          (0 positional arguments)
#             5574 POP_TOP
# 
# 13707:        5576 LOAD_FAST            (self)
#             5578 LOAD_ATTR            (power_group)
#             5580 LOAD_METHOD          (show)
#             5582 CALL_METHOD          (0 positional arguments)
#             5584 POP_TOP
# 
# 13709:        5586 LOAD_FAST            (self)
#             5588 LOAD_ATTR            (canvas_scene)
#             5590 LOAD_ATTR            (selected_icon)
#             5592 LOAD_ATTR            (orientation)
#             5594 LOAD_GLOBAL          (Qt)
#             5596 LOAD_ATTR            (Vertical)
#             5598 COMPARE_OP           (==)
#             5600 EXTENDED_ARG         (5376)
#             5602 POP_JUMP_IF_FALSE    (to 5618)
# 
# 13710:        5604 LOAD_FAST            (self)
#             5606 LOAD_ATTR            (vertical_radio)
#             5608 LOAD_METHOD          (setChecked)
#             5610 LOAD_CONST           (True)
#             5612 CALL_METHOD          (1 positional argument)
#             5614 POP_TOP
#             5616 JUMP_FORWARD         (to 5630)
# 
# 13712:     >> 5618 LOAD_FAST            (self)
#             5620 LOAD_ATTR            (horizontal_radio)
#             5622 LOAD_METHOD          (setChecked)
#             5624 LOAD_CONST           (True)
#             5626 CALL_METHOD          (1 positional argument)
#             5628 POP_TOP
# 
# 13714:     >> 5630 LOAD_FAST            (self)
#             5632 LOAD_ATTR            (canvas_scene)
#             5634 LOAD_ATTR            (selected_icon)
#             5636 LOAD_ATTR            (power_direction)
#             5638 LOAD_CONST           (1)
#             5640 COMPARE_OP           (==)
#             5642 EXTENDED_ARG         (5632)
#             5644 POP_JUMP_IF_FALSE    (to 5660)
# 
# 13715:        5646 LOAD_FAST            (self)
#             5648 LOAD_ATTR            (forward_radio)
#             5650 LOAD_METHOD          (setChecked)
#             5652 LOAD_CONST           (True)
#             5654 CALL_METHOD          (1 positional argument)
#             5656 POP_TOP
#             5658 JUMP_FORWARD         (to 5672)
# 
# 13717:     >> 5660 LOAD_FAST            (self)
#             5662 LOAD_ATTR            (reverse_radio)
#             5664 LOAD_METHOD          (setChecked)
#             5666 LOAD_CONST           (True)
#             5668 CALL_METHOD          (1 positional argument)
#             5670 POP_TOP
#          >> 5672 JUMP_FORWARD         (to 5706)
# 
# 13719:     >> 5674 LOAD_FAST            (self)
#             5676 LOAD_ATTR            (orientation_group)
#             5678 LOAD_METHOD          (hide)
#             5680 CALL_METHOD          (0 positional arguments)
#             5682 POP_TOP
# 
# 13720:        5684 LOAD_FAST            (self)
#             5686 LOAD_ATTR            (power_group)
#             5688 LOAD_METHOD          (hide)
#             5690 CALL_METHOD          (0 positional arguments)
#             5692 POP_TOP
# 
# 13721:        5694 LOAD_CONST           (255)
#             5696 STORE_GLOBAL         (device_xuanzhong_flag)
# 
# 13722:        5698 LOAD_GLOBAL          (print)
#             5700 LOAD_CONST           ("图标取消选择")
#             5702 CALL_FUNCTION        (1 positional argument)
#             5704 POP_TOP
#          >> 5706 LOAD_CONST           (None)
#             5708 RETURN_VALUE
# 
# 
# ==== .ControlPanel.update_color_btn (linha 13724)
self.color_btn.setStyleSheet(f"background-color: rgb({color[0]}, {color[1]}, {color[2]});border: none; color: white;")
# ==== .ControlPanel.set_button_color (linha 13731)
button.setStyleSheet(f"background-color: rgb({color[0]}, {color[1]}, {color[2]});border: none; color: white;")
# ==== .ControlPanel.set_effect_color (linha 13738)
effect_type = self.effect_manager.current_effect
effect = self.effect_manager.effects[effect_type]
color = QColorDialog.getColor()
if color.isValid():
    rgb = [
     color.red(), color.green(), color.blue()]
    effect["color"] = rgb
    self.update_color_btn(rgb)
    self.effect_manager.save_config()
# ==== .ControlPanel.set_effect_width (linha 13750)
effect_type = self.effect_manager.current_effect
effect = self.effect_manager.effects[effect_type]
effect["width"] = value
self.width_label_txt.setText(f"{value}")
self.effect_manager.save_config()
# ==== .ControlPanel.set_effect_height (linha 13759)
effect_type = self.effect_manager.current_effect
effect = self.effect_manager.effects[effect_type]
effect["height"] = value
self.height_label_txt.setText(f"{value}")
self.effect_manager.save_config()
# ==== .ControlPanel.set_multi_color (linha 13768)
effect_type = self.effect_manager.current_effect
effect = self.effect_manager.effects[effect_type]
color = QColorDialog.getColor()
if color.isValid():
    rgb = [
     color.red(), color.green(), color.blue()]
    effect["colors"][index] = rgb
    self.set_button_color(self.color_btns[index], rgb)
    self.effect_manager.save_config()
# ==== .ControlPanel.update_effect_params (linha 13780)
speed = self.speed_slider.value()
self.speed_label.setText(f"{speed}")
self.effect_manager.effects[self.effect_manager.current_effect]["speed"] = speed
self.effect_manager.save_config()
print("update_effect_params")
# ==== .ControlPanel.update_effect_liangdu (linha 13788)
liangdu = self.liangdu_slider.value()
self.liangdu_label.setText(f"{liangdu}")
self.effect_manager.effect_brightness = liangdu
self.effect_manager.save_config()
print("update_effect_liangdu")