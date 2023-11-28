<script setup lang="ts">
    import { onMounted, ref } from "vue";
    import * as monaco from "monaco-editor";
    import MonacoEditor from "./MonacoEditor.vue";

    //组件配置
    const config = defineProps({
        baseAddress: String
    });
    const nugetRef = ref();
    const codeRef = ref();
    const inputRef = ref();
    const previewRef = ref();
    const versionValue = ref("NET6_0");
    const methdoname = ref("");
    const versions = [{ value: "NET6_0", label: ".net 6" }];

    //编辑器配置
    const editorConfig = {
        code: {
            value: `using System.Collections;
using System.Collections.Generic;
public class Program
{
    public string Invoke(string str){
        return "Invoke Result:"+str;
    }
    public KeyValuePair<string, string> GetKVResult(string str){
        return new KeyValuePair<string, string>("Invoke Result",str);
    }
}`,
            language: "csharp",
            automaticLayout: true, // 自动布局
            readOnly: false, // 是否为只读模式
            contextmenu: true, // 上下文菜单
            minimap: {
                enabled: false, // 代码缩略图
            },
            lightbulb: {
                enabled: false, // 快速修复功能
            },
            scrollBeyondLastLine: true, //滚动超出最后一行
        },
        preview: {
            value: ``,
            language: "plaintext",
            automaticLayout: true, // 自动布局
            readOnly: false, // 是否为只读模式
            contextmenu: false, // 上下文菜单
            wordWrap: "on", //自动换行
            minimap: {
                enabled: false, // 代码缩略图
            },
            lightbulb: {
                enabled: false, // 快速修复功能
            },
            scrollBeyondLastLine: true, //滚动超出最后一行
        },
        input: {
            value: "",
            language: "plaintext",
            wordWrap: "on",
            automaticLayout: true, // 自动布局
            readOnly: false, // 是否为只读模式
            contextmenu: false, // 上下文菜单
            minimap: {
                enabled: false, // 代码缩略图
            },
            lightbulb: {
                enabled: false, // 快速修复功能
            },
            find: {
                addExtraSpaceOnTop: false,
                autoFindInSelection: "never",
                seedSearchStringFromSelection: false,
            },
            scrollBeyondLastLine: true, //滚动超出最后一行
        },
        nuget: {
            value: "",
            language: "csharp",
            wordWrap: "off",
            automaticLayout: true, // 自动布局
            readOnly: false, // 是否为只读模式
            contextmenu: false, // 上下文菜单
            minimap: {
                enabled: false, // 代码缩略图
            },
            lightbulb: {
                enabled: false, // 快速修复功能
            },
            find: {
                addExtraSpaceOnTop: false,
                autoFindInSelection: "never",
                seedSearchStringFromSelection: false,
            },
            scrollBeyondLastLine: true, //滚动超出最后一行
        },
    };
    //智能感知
    let Intellisense = (e) => { };
    //执行代码
    let ExecuteCode = (e) => {
        let scriptValue = codeRef.value.getEditor().getValue();
        let inputValue = inputRef.value.getEditor().getValue();
        let headers = new Headers();
        headers.append("Content-Type", "application/x-www-form-urlencoded");
        let formbody = new URLSearchParams();
        formbody.append("script", scriptValue);
        formbody.append("inparam", inputValue);
        formbody.append("methodname", methdoname.value);

        fetch(`${config.baseAddress}/script/execute`, {
            method: "Post",
            mode: "cors",
            headers: headers,
            cache: "default",
            body: formbody,
        })
            .then((res) => res.text())
            .then((data) => {
                previewRef.value.getEditor().setValue(data);
            })
            .catch((error) => {
                previewRef.value
                    .getEditor()
                    .setValue(
                        `${new Date().toLocaleDateString()} ${new Date().toLocaleTimeString()}\t=>\tThere has been a problem with your fetch operation:\n${error.stack
                        }`
                    );
            });
    };

    let codeFormat = (editor: monaco.editor.ICodeEditor) => {
        let scriptValue = editor.getValue();
        let headers = new Headers();
        headers.append("Content-Type", "application/x-www-form-urlencoded");
        let formbody = new URLSearchParams();
        formbody.append("script", scriptValue);

        fetch(`${config.baseAddress}/script/format`, {
            method: "Post",
            mode: "cors",
            headers: headers,
            cache: "default",
            body: formbody,
        })
            .then((res) => res.json())
            .then((data) => {
                if (data.error) {
                    previewRef.value
                        .getEditor()
                        .setValue(data.error);
                } else {
                    editor.pushUndoStop()
                    editor.executeEdits(null, [{
                        text: data.code,
                        range:editor.getModel().getFullModelRange()
                    }]);
                    editor.pushUndoStop()
                }
            })
            .catch((error) => {
                previewRef.value
                    .getEditor()
                    .setValue(
                        `${new Date().toLocaleDateString()} ${new Date().toLocaleTimeString()}\t=>\tThere has been a problem with your fetch operation:\n${error.stack
                        }`
                    );
            });
    };
    let codeFormatAction = {
        id: "format-code",
        label: "格式化代码",
        contextMenuOrder: 0,
        contextMenuGroupId: "1_modification",
        keybindings: [
            monaco.KeyMod.chord(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyK, monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyD)
        ],
        run: codeFormat,
    };

    let draggingX = 0;
    let draggingY = 0;
    let dragging = 0;

    onMounted(() => {
        document.onmouseup = (e) => {
            dragging
                ? draggingX == 0
                    ? clearJSEvents(resizeY)
                    : clearJSEvents(resize)
                : "";
        };
        codeRef.value.getEditor().addAction(codeFormatAction);
        codeRef.value.getEditor().onDidChangeModelContent((e) => {
            // do somethings ...
            // console.log("内容变更！");
        });
    });

    let separateMousedown = (e) => {
        e.preventDefault();
        dragging = 1;
        draggingX = e.screenX;
        toggleContentMouseMoveListener(resize);
    };

    let toggleContentMouseMoveListener = (func) => {
        if (dragging) {
            document.querySelector(".content").addEventListener("mousemove", func);
        } else {
            document.querySelector(".content").removeEventListener("mousemove", func);
        }
    };

    let resize = (e) => {
        if (dragging) {
            var left =
                document.querySelector(".left").clientWidth + (e.screenX - draggingX);
            var right =
                document.querySelector(".right").clientWidth - (e.screenX - draggingX);
            if (left < 400 || right < 400) {
                return;
            }
            console.debug("left", left, "right", right);
            (<HTMLElement>document.querySelector(".left")).style.setProperty(
                "width",
                left + "px"
            );
            (<HTMLElement>document.querySelector(".right")).style.setProperty(
                "width",
                right + "px"
            );
            draggingX = e.screenX;
        }
    };

    let inputLabelMousedown = (e) => {
        e.preventDefault();
        if (document.querySelector(".inputEditor .label") === e.target) {
            dragging = 1;
            draggingY = e.screenY;
            toggleContentMouseMoveListener(resizeY);
        }
    };

    let resizeY = (e) => {
        if (dragging) {
            var codeheight =
                document.querySelector(".codeEditor").clientHeight +
                (e.screenY - draggingY);
            var inputheight =
                document.querySelector(".inputEditor").clientHeight -
                (e.screenY - draggingY);
            if (codeheight < 300 || inputheight < 80) {
                return;
            }
            // console.debug("codeheight", codeheight, "inputheight", inputheight);
            (<HTMLElement>document.querySelector(".codeEditor")).style.setProperty(
                "height",
                codeheight + "px"
            );
            (<HTMLElement>document.querySelector(".inputEditor")).style.setProperty(
                "height",
                inputheight + "px"
            );
            draggingY = e.screenY;
        }
    };
    let clearJSEvents = (func) => {
        dragging = 0;
        draggingX = 0;
        draggingY = 0;
        toggleContentMouseMoveListener(func);
    };
</script>
<template>
    <div class="content">
        <div class="left">
            <div class="header">
                <el-row>
                    <el-col :span="8">
                        <el-text>C#</el-text>
                    </el-col>
                    <el-col :span="16" style="min-width: 350px">
                        <el-row :gutter="20" justify="end" align="center">
                            <el-col :span="8">
                                <el-select v-model="versionValue" placeholder="Select">
                                    <el-option v-for="item in versions"
                                               :key="item.value"
                                               :label="item.label"
                                               :value="item.value" />
                                </el-select>
                            </el-col>
                            <el-col :span="12">
                                <el-input v-model="methdoname"
                                          placeholder="Please input Run Methodname"></el-input>
                            </el-col>
                            <el-col :span="4">
                                <el-button id="executeBtn" @click="ExecuteCode" type="primary">Run</el-button>
                            </el-col>
                        </el-row>
                    </el-col>
                </el-row>
            </div>
            <div class="nugetEditor">
                <MonacoEditor id="nuget"
                              label="Nuget Reference"
                              :config="editorConfig.nuget"
                              ref="nugetRef" />
            </div>
            <div class="codeEditor">
                <MonacoEditor id="code"
                              label="Code"
                              :config="editorConfig.code"
                              ref="codeRef" />
            </div>
            <div class="inputEditor" @mousedown="inputLabelMousedown">
                <MonacoEditor id="input"
                              label="Input"
                              :config="editorConfig.input"
                              ref="inputRef" />
            </div>
        </div>
        <div class="separate" @mousedown="separateMousedown">
            <div class="separateline"></div>
        </div>
        <div class="right">
            <div class="previewEditor">
                <MonacoEditor id="preview"
                              label="Output"
                              ref="previewRef"
                              :config="editorConfig.preview" />
            </div>
        </div>
    </div>
</template>

<style scoped>
    .content {
        box-shadow: 0 0 1px 1px #e4e4e4;
        background-color: white;
        display: flex;
        flex-flow: row nowrap;
        padding: 10px;
        height: calc(100% - 16px);
        width: calc(100% - 10px - 10px);
        min-width: 900px;
    }

    .header {
        min-height: 32px;
    }

    .left {
        display: flex;
        flex-flow: column nowrap;
        width: 50%;
    }

    .nugetEditor {
        height: 120px;
        min-height: 50px;
    }

    .codeEditor {
        height: calc(100% - 32px - 120px - 120px);
        min-height: 100px;
    }

    .inputEditor {
        height: 120px;
        min-height: 50px;
    }

    .previewEditor {
        height: 100%;
    }

    .separate {
        display: flex;
        flex: 0 0 10px;
        justify-content: center;
    }

        .separate:hover {
            cursor: ew-resize;
        }

    .separateline {
        border-right: 2px solid #dedede;
    }

    .right {
        flex: 1;
    }
</style>
<style>
    .inputEditor .label:first-child {
        cursor: ns-resize;
    }
</style>