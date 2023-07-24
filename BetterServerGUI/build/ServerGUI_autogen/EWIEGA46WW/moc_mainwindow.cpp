/****************************************************************************
** Meta object code from reading C++ file 'mainwindow.h'
**
** Created by: The Qt Meta Object Compiler version 67 (Qt 5.12.8)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include "mainwindow.h"
#include <QtCore/qbytearray.h>
#include <QtCore/qmetatype.h>
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'mainwindow.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 67
#error "This file was generated using the moc from 5.12.8. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
QT_WARNING_PUSH
QT_WARNING_DISABLE_DEPRECATED
struct qt_meta_stringdata_MainWindow_t {
    QByteArrayData data[20];
    char stringdata0[354];
};
#define QT_MOC_LITERAL(idx, ofs, len) \
    Q_STATIC_BYTE_ARRAY_DATA_HEADER_INITIALIZER_WITH_OFFSET(len, \
    qptrdiff(offsetof(qt_meta_stringdata_MainWindow_t, stringdata0) + ofs \
        - idx * sizeof(QByteArrayData)) \
    )
static const qt_meta_stringdata_MainWindow_t qt_meta_stringdata_MainWindow = {
    {
QT_MOC_LITERAL(0, 0, 10), // "MainWindow"
QT_MOC_LITERAL(1, 11, 31), // "on_comboBox_currentIndexChanged"
QT_MOC_LITERAL(2, 43, 0), // ""
QT_MOC_LITERAL(3, 44, 5), // "index"
QT_MOC_LITERAL(4, 50, 21), // "on_mapSet_itemClicked"
QT_MOC_LITERAL(5, 72, 16), // "QListWidgetItem*"
QT_MOC_LITERAL(6, 89, 4), // "item"
QT_MOC_LITERAL(7, 94, 21), // "on_mapSetOpen_clicked"
QT_MOC_LITERAL(8, 116, 21), // "on_mapSetSave_clicked"
QT_MOC_LITERAL(9, 138, 21), // "on_playersContextMenu"
QT_MOC_LITERAL(10, 160, 18), // "on_mapsContextMenu"
QT_MOC_LITERAL(11, 179, 15), // "on_mapsCheckAll"
QT_MOC_LITERAL(12, 195, 17), // "on_mapsUncheckAll"
QT_MOC_LITERAL(13, 213, 14), // "on_player_kick"
QT_MOC_LITERAL(14, 228, 13), // "on_player_ban"
QT_MOC_LITERAL(15, 242, 24), // "on_banListDelete_clicked"
QT_MOC_LITERAL(16, 267, 22), // "on_backToLobby_clicked"
QT_MOC_LITERAL(17, 290, 17), // "on_exeWin_clicked"
QT_MOC_LITERAL(18, 308, 23), // "on_pushButton_2_clicked"
QT_MOC_LITERAL(19, 332, 21) // "on_pushButton_clicked"

    },
    "MainWindow\0on_comboBox_currentIndexChanged\0"
    "\0index\0on_mapSet_itemClicked\0"
    "QListWidgetItem*\0item\0on_mapSetOpen_clicked\0"
    "on_mapSetSave_clicked\0on_playersContextMenu\0"
    "on_mapsContextMenu\0on_mapsCheckAll\0"
    "on_mapsUncheckAll\0on_player_kick\0"
    "on_player_ban\0on_banListDelete_clicked\0"
    "on_backToLobby_clicked\0on_exeWin_clicked\0"
    "on_pushButton_2_clicked\0on_pushButton_clicked"
};
#undef QT_MOC_LITERAL

static const uint qt_meta_data_MainWindow[] = {

 // content:
       8,       // revision
       0,       // classname
       0,    0, // classinfo
      15,   14, // methods
       0,    0, // properties
       0,    0, // enums/sets
       0,    0, // constructors
       0,       // flags
       0,       // signalCount

 // slots: name, argc, parameters, tag, flags
       1,    1,   89,    2, 0x08 /* Private */,
       4,    1,   92,    2, 0x08 /* Private */,
       7,    0,   95,    2, 0x08 /* Private */,
       8,    0,   96,    2, 0x08 /* Private */,
       9,    1,   97,    2, 0x08 /* Private */,
      10,    1,  100,    2, 0x08 /* Private */,
      11,    0,  103,    2, 0x08 /* Private */,
      12,    0,  104,    2, 0x08 /* Private */,
      13,    0,  105,    2, 0x08 /* Private */,
      14,    0,  106,    2, 0x08 /* Private */,
      15,    0,  107,    2, 0x08 /* Private */,
      16,    0,  108,    2, 0x08 /* Private */,
      17,    0,  109,    2, 0x08 /* Private */,
      18,    0,  110,    2, 0x08 /* Private */,
      19,    0,  111,    2, 0x08 /* Private */,

 // slots: parameters
    QMetaType::Void, QMetaType::Int,    3,
    QMetaType::Void, 0x80000000 | 5,    6,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void, QMetaType::QPoint,    2,
    QMetaType::Void, QMetaType::QPoint,    2,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,

       0        // eod
};

void MainWindow::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        auto *_t = static_cast<MainWindow *>(_o);
        Q_UNUSED(_t)
        switch (_id) {
        case 0: _t->on_comboBox_currentIndexChanged((*reinterpret_cast< int(*)>(_a[1]))); break;
        case 1: _t->on_mapSet_itemClicked((*reinterpret_cast< QListWidgetItem*(*)>(_a[1]))); break;
        case 2: _t->on_mapSetOpen_clicked(); break;
        case 3: _t->on_mapSetSave_clicked(); break;
        case 4: _t->on_playersContextMenu((*reinterpret_cast< const QPoint(*)>(_a[1]))); break;
        case 5: _t->on_mapsContextMenu((*reinterpret_cast< const QPoint(*)>(_a[1]))); break;
        case 6: _t->on_mapsCheckAll(); break;
        case 7: _t->on_mapsUncheckAll(); break;
        case 8: _t->on_player_kick(); break;
        case 9: _t->on_player_ban(); break;
        case 10: _t->on_banListDelete_clicked(); break;
        case 11: _t->on_backToLobby_clicked(); break;
        case 12: _t->on_exeWin_clicked(); break;
        case 13: _t->on_pushButton_2_clicked(); break;
        case 14: _t->on_pushButton_clicked(); break;
        default: ;
        }
    }
}

QT_INIT_METAOBJECT const QMetaObject MainWindow::staticMetaObject = { {
    &QMainWindow::staticMetaObject,
    qt_meta_stringdata_MainWindow.data,
    qt_meta_data_MainWindow,
    qt_static_metacall,
    nullptr,
    nullptr
} };


const QMetaObject *MainWindow::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->dynamicMetaObject() : &staticMetaObject;
}

void *MainWindow::qt_metacast(const char *_clname)
{
    if (!_clname) return nullptr;
    if (!strcmp(_clname, qt_meta_stringdata_MainWindow.stringdata0))
        return static_cast<void*>(this);
    return QMainWindow::qt_metacast(_clname);
}

int MainWindow::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QMainWindow::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 15)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 15;
    } else if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        if (_id < 15)
            *reinterpret_cast<int*>(_a[0]) = -1;
        _id -= 15;
    }
    return _id;
}
QT_WARNING_POP
QT_END_MOC_NAMESPACE
