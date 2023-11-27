<script setup>
import * as monaco from "monaco-editor";
import { ref, onBeforeUpdate, onMounted, onBeforeMount } from "vue";

const props = defineProps({
  label: String,
  id: String,
  className: String,
  config: Object,
  heightValue: String,
});
let editorInstance = ref();
const defaultConfig = {
  value: `public class Program{
        public static void Main(string[] args){
            Console.WriteLine("Hello Word!");
        }
    }`,
  language: "csharp",
  automaticLayout: true, // 自动布局
  readOnly: false, // 是否为只读模式
  contextmenu: true, // 上下文菜单
  minimap: {
    enabled: true, // 代码缩略图
  },
  lightbulb: {
    enabled: false, // 快速修复功能
  },
  scrollBeyondLastLine: true, //滚动超出最后一行
};

onBeforeMount(() => {
  console.debug(`the component is on Before Mount.`);
});

onMounted(() => {
  console.debug(`the component is now mounted.`);

  editorInstance = monaco.editor.create(
    document.getElementById(props.id),
    props.config ?? defaultConfig
  );
});
onBeforeUpdate(() => {
  console.debug(`the component is on Before Update.`);
});

const getEditor = () => {
  return editorInstance;
};
defineExpose({
  getEditor,
});
</script>

<template>
  <div class="editorComponents">
    <div :id="id + '_label'" class="label">{{ label }}：</div>
    <div class="editor" :id="id"></div>
  </div>
</template>

<style scoped>
.editorComponents {
  height: 100%;
  width: 100%;
}
.editorComponents .label {
  height: 20px;
  line-height: 16px;
}
.editor {
  border: 1px solid grey;
  height: calc(100% - 20px);
}
</style>