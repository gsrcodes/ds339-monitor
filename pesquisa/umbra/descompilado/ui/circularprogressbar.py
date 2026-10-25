# decompyle3 version 3.9.3
# Python bytecode version base 3.8.0
# Decompiled from: Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# Embedded file name: pyside2_kittle_ui\circularprogressbar.py
from PySide2.QtWidgets import QWidget, QApplication, QVBoxLayout, QSlider, QLabel
from PySide2.QtGui import QPainter, QColor, QPen, QFont
from PySide2.QtCore import Qt, QRectF, QSize, QPoint
import sys

class CircularProgressBar(QWidget):

    def __init__(self, parent=None):
        super().__init__(parent)
        self._value = 0
        self._min = 0
        self._max = 100
        self._font_percentage = QFont("Arial")
        self._font_percentage.setPixelSize(25)
        self._font_text = QFont("Arial")
        self._font_text.setPixelSize(20)
        self._text = "CPU占用率"
        self.resize(220, 220)
        self.setMinimumSize(150, 150)
        self._color = QColor(0, 170, 255)

    def sizeHint(self):
        return QSize(220, 220)

    def setValue(self, val):
        if val < self._min:
            val = self._min
        elif val > self._max:
            val = self._max
        self._value = val
        self.update()

    def value(self):
        return self._value

    def setText(self, text):
        self._text = text
        self.update()

    def setColor(self, color: QColor):
        """设置进度条颜色"""
        if isinstance(color, QColor):
            self._color = color
            self.update()
        else:
            raise TypeError("color must be a QColor instance")

    def paintEvent1(self, event):
        width = self.width()
        height = self.height()
        margin = 25
        radius = min(width, height) / 2 - margin
        painter = QPainter(self)
        painter.setRenderHints(QPainter.Antialiasing | QPainter.HighQualityAntialiasing)
        painter.translate(width / 2, height / 2)
        bg_pen = QPen(QColor(220, 220, 220, 100), 5)
        painter.setPen(bg_pen)
        painter.drawEllipse(QPoint(0, 0), radius, radius)
        angle_span = int(-360 * (self._value - self._min) / (self._max - self._min))
        circle_rect = QRectF(-radius, -radius, radius * 2, radius * 2)
        progress_pen_width = 10
        progress_pen = QPen(self._color, progress_pen_width)
        progress_pen.setCapStyle(Qt.RoundCap)
        painter.setPen(progress_pen)
        painter.drawArc(circle_rect, 1440, angle_span * 16)
        painter.setPen(QColor(255, 255, 255))
        painter.setFont(self._font_percentage)
        percent_text = f"{int(self._value)}%"
        rect_percent = QRectF(-radius, -radius / 2 - 10, radius * 2, radius)
        painter.drawText(rect_percent, Qt.AlignHCenter | Qt.AlignVCenter, percent_text)
        painter.setFont(self._font_text)
        rect_text = QRectF(-radius, radius / 4, radius * 2, radius / 2)
        painter.drawText(rect_text, Qt.AlignHCenter | Qt.AlignTop, self._text)
        painter.end()

    def paintEvent(self, event):
        width = self.width()
        height = self.height()
        margin = 25
        radius = min(width, height) / 2 - margin
        painter = QPainter(self)
        painter.setRenderHints(QPainter.Antialiasing | QPainter.HighQualityAntialiasing)
        painter.translate(width / 2, height / 2)
        arc_width = 4
        bg_pen = QPen(QColor(127, 127, 127), arc_width)
        painter.setPen(bg_pen)
        painter.drawEllipse(QPoint(0, 0), radius, radius)
        angle_span = int(-360 * (self._value - self._min) / (self._max - self._min))
        circle_rect = QRectF(-radius, -radius, radius * 2, radius * 2)
        glow_steps = 10
        max_alpha = 10
        min_alpha = 10
        for i in range(glow_steps):
            alpha = int(max_alpha - (max_alpha - min_alpha) * (i / glow_steps))
            alpha = max(alpha, 0)
            w = arc_width + i * 2.0
            glow_color = QColor(self._color)
            glow_color.setAlpha(alpha)
            pen = QPen(glow_color, w)
            pen.setCapStyle(Qt.RoundCap)
            painter.setPen(pen)
            extend = i * 1.0
            start = 1440 + extend
            span = angle_span * 16 - extend * 2
            painter.drawArc(circle_rect, start, span)

        main_pen = QPen(self._color, arc_width)
        main_pen.setCapStyle(Qt.RoundCap)
        painter.setPen(main_pen)
        painter.drawArc(circle_rect, 1440, angle_span * 16)
        painter.setPen(QColor(255, 255, 255))
        painter.setFont(self._font_percentage)
        percent_text = f"{int(self._value)}%"
        rect_percent = QRectF(-radius, -radius / 2 - 10, radius * 2, radius)
        painter.drawText(rect_percent, Qt.AlignHCenter | Qt.AlignVCenter, percent_text)
        painter.setFont(self._font_text)
        rect_text = QRectF(-radius, radius / 4, radius * 2, radius / 2)
        painter.drawText(rect_text, Qt.AlignHCenter | Qt.AlignTop, self._text)
        painter.end()


class Demo(QWidget):

    def __init__(self):
        super().__init__()
        self.setWindowTitle("圆形进度条 Demo - PySide2")
        self.resize(300, 400)
        self.progress_bar = CircularProgressBar()
        self.progress_bar.setText("CPU负载")
        self.progress_bar.setColor(QColor(0, 213, 255))
        self.slider = QSlider(Qt.Horizontal)
        self.slider.setRange(0, 100)
        self.slider.setValue(12)
        self.slider.valueChanged.connect(self.onSliderChange)
        self.label = QLabel("进度：12%")
        self.label.setAlignment(Qt.AlignCenter)
        layout = QVBoxLayout()
        layout.addWidget((self.progress_bar), alignment=(Qt.AlignCenter))
        layout.addWidget(self.label)
        layout.addWidget(self.slider)
        self.setLayout(layout)

    def onSliderChange(self, val):
        self.progress_bar.setValue(val)
        self.label.setText(f"进度：{val}%")


if __name__ == "__main__":
    app = QApplication(sys.argv)
    demo = Demo()
    demo.show()
    sys.exit(app.exec_())
