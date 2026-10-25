# decompyle3 version 3.9.3
# Python bytecode version base 3.8.0
# Decompiled from: Python 3.13.15 (tags/v3.13.15:4061bc4, Aug  5 2026, 13:05:39) [MSC v.1944 64 bit (AMD64)]
# Embedded file name: pyside2_kittle_ui\lable_right_botem_mengban.py
import sys
from PySide2.QtWidgets import QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, QPushButton, QCheckBox, QLabel
from PySide2.QtCore import Qt, QRectF, QPointF
from PySide2.QtGui import QPainter, QColor, QFont, QLinearGradient, QPainterPath

class Overlay(QWidget):
    __doc__ = "带右下角圆角的半透明蒙版控件"

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setAttribute(Qt.WA_TransparentForMouseEvents, False)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setWindowFlags(Qt.FramelessWindowHint)
        self.corner_radius = 30

    def paintEvent(self, event):
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)
        rect = QRectF(self.rect())
        r = self.corner_radius
        path = QPainterPath()
        path.moveTo(rect.topLeft())
        path.lineTo(rect.topRight())
        path.lineTo(rect.right(), rect.bottom() - r)
        path.quadTo(rect.right(), rect.bottom(), rect.right() - r, rect.bottom())
        path.lineTo(rect.left(), rect.bottom())
        path.lineTo(rect.left(), rect.top())
        path.closeSubpath()
        painter.fillPath(path, QColor(0, 0, 0, 200))

    def mousePressEvent(self, event):
        event.ignore()
