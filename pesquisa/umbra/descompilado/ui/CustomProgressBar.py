# decompyle3 version 3.9.3
# Python bytecode version base 3.8.0
# Decompiled from: Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# Embedded file name: pyside2_kittle_ui\CustomProgressBar.py
import sys
from PySide2.QtWidgets import QApplication, QWidget, QVBoxLayout, QHBoxLayout, QLabel, QPushButton
from PySide2.QtGui import QColor, QPainter, QBrush
from PySide2.QtCore import Qt, Property

class CustomProgressBar(QWidget):

    def __init__(self, parent=None):
        super().__init__(parent)
        self._value = 0
        self._max = 100
        self._bar_color = QColor("#0078d7")
        self._background_color = QColor(127, 127, 127)
        self._left_text = "左侧文字"
        self._right_text = "右侧文字"
        self.main_layout = QVBoxLayout(self)
        self.main_layout.setContentsMargins(5, 5, 5, 5)
        self.main_layout.setSpacing(5)
        self.text_layout = QHBoxLayout()
        self.text_layout.setContentsMargins(0, 0, 0, 0)
        self.text_layout.setSpacing(0)
        self.left_label = QLabel(self._left_text)
        self.left_label.setStyleSheet("color: white; font-size: 16px;")
        self.right_label = QLabel(self._right_text)
        self.right_label.setStyleSheet("color: white; font-size: 16px;")
        self.right_label.setAlignment(Qt.AlignRight | Qt.AlignVCenter)
        self.text_layout.addWidget(self.left_label, 1)
        self.text_layout.addWidget(self.right_label, 1)
        self.main_layout.addLayout(self.text_layout)
        self.setMinimumWidth(200)
        self.setMinimumHeight(50)
        self.setFixedHeight(70)

    def paintEvent(self, event):
        painter = QPainter(self)
        rect = self.rect()
        progress_bar_height = 4
        progress_bar_rect = rect.adjusted(5, 5, -5, -(rect.height() - progress_bar_height - 5))
        painter.setPen(Qt.NoPen)
        painter.setBrush(self._background_color)
        painter.drawRect(progress_bar_rect)
        progress_width = int(self._value / self._max * progress_bar_rect.width())
        if progress_width > 0:
            progress_rect = progress_bar_rect.adjusted(0, 0, -(progress_bar_rect.width() - progress_width), 0)
            painter.setBrush(self._bar_color)
            painter.drawRect(progress_rect)
        painter.end()

    def getValue(self):
        return self._value

    def setValue(self, value):
        if 0 <= value <= self._max:
            self._value = value
            self.update()
            self.updateLabels()

    def getMax(self):
        return self._max

    def setMax(self, max_value):
        if max_value > 0:
            self._max = max_value
            if self._value > max_value:
                self._value = max_value
            self.update()
            self.updateLabels()

    def getBarColor(self):
        return self._bar_color

    def setBarColor(self, color):
        if isinstance(color, QColor):
            self._bar_color = color
            self.update()

    def getBackgroundColor(self):
        return self._background_color

    def setBackgroundColor(self, color):
        if isinstance(color, QColor):
            self._background_color = color
            self.update()

    def getLeftText(self):
        return self._left_text

    def setLeftText(self, text):
        self._left_text = text
        self.left_label.setText(text)

    def getRightText(self):
        return self._right_text

    def setRightText(self, text):
        self._right_text = text
        self.right_label.setText(text)

    def updateLabels(self):
        pass

    value = Property(int, getValue, setValue)
    maximum = Property(int, getMax, setMax)
    barColor = Property(QColor, getBarColor, setBarColor)
    backgroundColor = Property(QColor, getBackgroundColor, setBackgroundColor)
    leftText = Property(str, getLeftText, setLeftText)
    rightText = Property(str, getRightText, setRightText)


class DemoWindow(QWidget):

    def __init__(self):
        super().__init__()
        self.setWindowTitle("自定义进度条 Demo")
        self.resize(400, 150)
        vbox = QVBoxLayout(self)
        self.progress = CustomProgressBar()
        vbox.addWidget(self.progress)
        btn_increase = QPushButton("增加进度")
        btn_increase.clicked.connect(self.increase_progress)
        vbox.addWidget(btn_increase)
        btn_decrease = QPushButton("减少进度")
        btn_decrease.clicked.connect(self.decrease_progress)
        vbox.addWidget(btn_decrease)
        btn_color = QPushButton("切换颜色")
        btn_color.clicked.connect(self.change_color)
        vbox.addWidget(btn_color)
        btn_text = QPushButton("修改文字")
        btn_text.clicked.connect(self.change_text)
        vbox.addWidget(btn_text)
        self.progress.leftText = "当前进度"
        self.progress.rightText = "0%"
        self.colors = [
         "#0078d7", "#e81123", "#107c10", "#ff8c00"]
        self.color_index = 0

    def increase_progress(self):
        val = min(self.progress.value + 10, self.progress.maximum)
        self.progress.value = val
        self.progress.rightText = f"{val}%"

    def decrease_progress(self):
        val = max(self.progress.value - 10, 0)
        self.progress.value = val
        self.progress.rightText = f"{val}%"

    def change_color(self):
        self.color_index = (self.color_index + 1) % len(self.colors)
        color = QColor(self.colors[self.color_index])
        self.progress.barColor = color

    def change_text(self):
        left_candidates = [
         "下载中", "上传中", "处理中", "完成"]
        right_candidates = ["请稍候", "等待", "成功", "100%"]
        import random
        self.progress.leftText = random.choice(left_candidates)
        self.progress.rightText = random.choice(right_candidates)


if __name__ == "__main__":
    app = QApplication(sys.argv)
    demo = DemoWindow()
    demo.show()
    sys.exit(app.exec_())
