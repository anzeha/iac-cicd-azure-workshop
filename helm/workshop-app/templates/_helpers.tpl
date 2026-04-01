{{- define "workshop-app.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "workshop-app.fullname" -}}
{{- printf "%s-%s" .Release.Name (include "workshop-app.name" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "workshop-app.labels" -}}
app.kubernetes.io/name: {{ include "workshop-app.name" . }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: nlb-workshop
{{- end -}}

{{- define "workshop-app.selectorLabels" -}}
app.kubernetes.io/name: {{ include "workshop-app.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{- define "workshop-app.secretName" -}}
{{- printf "%s-runtime" (include "workshop-app.fullname" .) -}}
{{- end -}}
